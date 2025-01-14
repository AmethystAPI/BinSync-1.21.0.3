using Godot;
using Networking;
using Riptide;

public partial class Slimeball : Enemy {
    [Export] public PackedScene ProjectileScene;
    [Export] public Node2D ProjectileOrigin;
    [Export] public PackedScene ChildScene;

    public override void _Ready() {
        base._Ready();

        NetworkPoint.Register(nameof(SummonRpc), SummonRpc);
    }

    public override void AddStates() {
        base.AddStates();

        _stateMachine.Add(new Idle("idle", this) {
            Interval = new Vector2(1.2f, 1.6f)
        });

        _stateMachine.Add(new JumpAttack("attack", this) {
            OnJump = direction => {
                SquashAndStretch.Trigger(new Vector2(0.6f, 1.4f), 4f);
            },
            OnLand = direction => {
                SquashAndStretch.Trigger(new Vector2(1.4f, 0.6f), 10f);

                Camera.Shake(0.2f);

                Projectile projectile = ProjectileScene.Instantiate<Projectile>();

                projectile.Source = this;

                GetParent().AddChild(projectile);

                projectile.GlobalPosition = ProjectileOrigin.GlobalPosition;
            }
        });
    }

    public override bool CanDamage(Projectile projectile) {
        if (!base.CanDamage(projectile)) return false;

        if (_stateMachine.CurrentState == "attack") return false;

        return true;
    }

    public override void SyncPosition(float delta) {
        if (NetworkPoint.IsOwner) {
            _networkedPosition.Value = GlobalPosition;
        } else if (_stateMachine.CurrentState != "attack" && _networkedPosition.Synced) {
            if (_networkedPosition.Value.DistanceSquaredTo(GlobalPosition) > 64) GlobalPosition = _networkedPosition.Value;

            GlobalPosition = GlobalPosition.Lerp(_networkedPosition.Value, delta * 20.0f);
        }
    }

    protected override void Die() {
        if (!NetworkManager.IsHost) return;

        for (int i = 0; i < 3; i++) {
            NetworkPoint.SendRpcToClientsFast(nameof(SummonRpc), message => {
                message.AddFloat(Game.RandomNumberGenerator.RandfRange(-8, 8));
                message.AddFloat(Game.RandomNumberGenerator.RandfRange(-8, 8));
            });
        }
    }

    private void SummonRpc(Message message) {
        Enemy enemy = NetworkManager.SpawnNetworkSafe<Enemy>(ChildScene, "Summon");

        GetParent().AddChild(enemy);

        enemy.GlobalPosition = GlobalPosition + new Vector2(message.GetFloat(), message.GetFloat());

        enemy.Activate();

        OnSummonedEnemy?.Invoke(enemy);
    }
}
