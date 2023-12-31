using Godot;

public partial class StoneGolemRoll : State
{
    [Export] public float Speed = 10f;
    [Export] public PackedScene ProjectileScene;
    [Export] public Node2D ProjectileOrigin;

    private StoneGolem _stoneGolem;
    private Vector2 _direction;
    private Projectile _projectile;

    public override void _Ready()
    {
        _stoneGolem = GetParent().GetParent<StoneGolem>();
    }

    public override void Enter()
    {
        if (Player.AlivePlayers.Count == 0)
        {
            GoToState("Idle");

            return;
        }

        Vector2 target = Player.AlivePlayers[0].GlobalPosition;

        foreach (Player player in Player.AlivePlayers)
        {
            if (_stoneGolem.GlobalPosition.DistanceTo(player.GlobalPosition) >= _stoneGolem.GlobalPosition.DistanceTo(target)) continue;

            target = player.GlobalPosition;
        }

        _direction = (target - _stoneGolem.GlobalPosition).Normalized();

        _projectile = ProjectileScene.Instantiate<Projectile>();

        _projectile.Source = _stoneGolem;

        _stoneGolem.AddChild(_projectile);

        _projectile.GlobalPosition = ProjectileOrigin.GlobalPosition;
        _projectile.Position += _direction * 4f;

        _projectile.LookAt(_projectile.GlobalPosition + _direction);

        _projectile.Destroyed += () => GoToState("Idle");
    }

    public override void PhsysicsUpdate(float delta)
    {
        _stoneGolem.Velocity = _direction * Speed;

        _stoneGolem.MoveAndSlide();
    }

    public override void Exit()
    {
        if (IsInstanceValid(_projectile)) _projectile.QueueFree();
    }
}
