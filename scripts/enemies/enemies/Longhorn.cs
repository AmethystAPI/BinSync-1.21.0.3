using Godot;
using Networking;
using Riptide;

public partial class Longhorn : Enemy {
    [Export] public PackedScene ProjectileScene;
    [Export] public Node2D ProjectileOrigin;

    public override void _Ready() {
        base._Ready();

        NetworkPoint.Register(nameof(SetRandomSeedRpc), SetRandomSeedRpc);
    }

    public override void AddStates() {
        base.AddStates();

        _stateMachine.Add(new Idle("idle", this) {
            AttackState = "telegraph_attack",
            Interval = new Vector2(2f, 3f),
            Movement = delta => {
                Vector2 target = GetWeightedTargets()[0].Player.GlobalPosition;
                Face(target);

                Velocity = Knockback;

                MoveAndSlide();
            }
        });

        _stateMachine.Add(new Telegraph("telegraph_attack", this, "attack") {
            OnEnter = () => {
                SquashAndStretch.Trigger(new Vector2(0.6f, 1.4f), 8f);
            }
        });

        _stateMachine.Add(new DashAttack("attack", this) {
            Speed = 200,
            Variance = 0f,
            OnDash = (direction, target) => {
                SquashAndStretch.Trigger(new Vector2(1.4f, 0.6f), 8f);

                Face(target);

                Projectile projectile = ProjectileScene.Instantiate<Projectile>();

                projectile.Source = this;

                AddChild(projectile);

                projectile.GlobalPosition = ProjectileOrigin.GlobalPosition;
                projectile.Position += direction * 12f;

                projectile.LookAt(projectile.GlobalPosition + direction);

                return projectile;
            },
            OnStop = () => {
                SquashAndStretch.Trigger(new Vector2(1.4f, 0.6f), 8f);
            }
        });

        if (NetworkManager.IsHost) NetworkPoint.SendRpcToClients(nameof(SetRandomSeedRpc), message => message.AddULong(_stateMachine.GetState<DashAttack>("attack").Random.Seed));
    }

    public override void SyncPosition(float delta) {
        if (NetworkPoint.IsOwner) {
            _networkedPosition.Value = GlobalPosition;
        } else if (_stateMachine.CurrentState != "attack" && _networkedPosition.Synced) {
            if (_networkedPosition.Value.DistanceSquaredTo(GlobalPosition) > 64) GlobalPosition = _networkedPosition.Value;

            GlobalPosition = GlobalPosition.Lerp(_networkedPosition.Value, delta * 20.0f);
        }
    }

    private void SetRandomSeedRpc(Message message) {
        _stateMachine.GetState<DashAttack>("attack").Random.Seed = message.GetULong();
    }
}
