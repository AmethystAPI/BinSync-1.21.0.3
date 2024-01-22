using Godot;
using Networking;
using Riptide;

public partial class CrowAggressive : State, NetworkPointUser {
    [Export] public float Speed = 10f;
    [Export] public float InverseInertia = 4f;
    [Export] public PackedScene ProjectileScene;
    [Export] public Node2D ProjectileOrigin;
    [Export] public Sprite2D Sprite;

    public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

    private Crow _crow;
    private Projectile _projectile;

    public override void _Ready() {
        _crow = GetParent().GetParent<Crow>();

        NetworkPoint.Setup(this);

        NetworkPoint.Register(nameof(ScaredRpc), ScaredRpc);

    }

    public override void Enter() {
        _projectile = ProjectileScene.Instantiate<Projectile>();

        _projectile.Source = _crow;

        ProjectileOrigin.AddChild(_projectile);

        _projectile.Destroyed += () => NetworkPoint.BounceRpcToClients(nameof(ScaredRpc));
    }

    public override void PhsysicsUpdate(float delta) {
        if (Player.AlivePlayers.Count == 0) {
            GoToState("Scared");

            return;
        }

        if (!_crow.NetworkPoint.IsOwner) return;

        Vector2 target = Player.AlivePlayers[0].GlobalPosition;

        foreach (Player player in Player.AlivePlayers) {
            if (_crow.GlobalPosition.DistanceTo(player.GlobalPosition) >= _crow.GlobalPosition.DistanceTo(target)) continue;

            target = player.GlobalPosition;
        }

        _crow.Velocity = _crow.Velocity.Slerp((target - _crow.GlobalPosition).Normalized() * Speed, InverseInertia * delta);

        Sprite.Scale = new Vector2(target.X >= _crow.GlobalPosition.X ? 1f : -1f, 1f);

        _crow.MoveAndSlide();
    }

    public override void Exit() {
        if (IsInstanceValid(_projectile)) _projectile.QueueFree();
    }

    private void ScaredRpc(Message message) {
        GoToState("Scared");
    }
}
