using Godot;

public class StoneGolemRoll : State {
    [Export] public float Speed = 10f;
    [Export] public PackedScene ProjectileScene;
    [Export] public Node2D ProjectileOrigin;
    [Export] public AnimationPlayer AnimationPlayer;
    [Export] public Sprite2D Sprite;
    [Export] public SquashAndStretch StartSquashAndStretch;
    [Export] public SquashAndStretch EndSquashAndStretch;

    public RandomNumberGenerator Random = new RandomNumberGenerator();

    private Enemy _enemy;
    private Vector2 _direction;
    private Projectile _projectile;

    public StoneGolemRoll(string name, Enemy enemy) : base(name) {
        _enemy = enemy;
    }

    public override void Enter() {
        StartSquashAndStretch.Activate();

        if (Player.AlivePlayers.Count == 0) {
            GoToState("Idle");

            return;
        }

        Vector2 target = _enemy.GetWeightedTargets()[0].Player.GlobalPosition;

        foreach (Player player in Player.AlivePlayers) {
            if (_enemy.GlobalPosition.DistanceTo(player.GlobalPosition) >= _enemy.GlobalPosition.DistanceTo(target)) continue;

            target = player.GlobalPosition;
        }

        _direction = (target - _enemy.GlobalPosition).Normalized();

        _direction = _direction.Rotated(Random.RandfRange(-Mathf.Pi / 6f, Mathf.Pi / 6f));

        _projectile = ProjectileScene.Instantiate<Projectile>();

        _projectile.Source = _enemy;

        _enemy.AddChild(_projectile);

        _projectile.GlobalPosition = ProjectileOrigin.GlobalPosition;
        _projectile.Position += _direction * 5f;

        _projectile.LookAt(_projectile.GlobalPosition + _direction);

        Sprite.Scale = new Vector2((target.X > _enemy.GlobalPosition.X ? 1f : -1f) * (target.Y > _enemy.GlobalPosition.Y ? 1f : -1f), 1f);

        if (target.Y > _enemy.GlobalPosition.Y) {
            AnimationPlayer.Play("roll");
        } else {
            AnimationPlayer.Play("roll_back");
        }

        _projectile.Destroyed += () => GoToState("Idle");
    }

    public override void PhsysicsUpdate(float delta) {
        _enemy.Velocity = _direction * Speed;

        _enemy.MoveAndSlide();
    }

    public override void Exit() {
        if (GodotObject.IsInstanceValid(_projectile)) _projectile.QueueFree();

        EndSquashAndStretch.Activate();
    }
}
