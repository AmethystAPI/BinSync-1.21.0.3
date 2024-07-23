using Godot;

public partial class SlimeJump : NodeState {
    [Export] public float Speed = 10f;
    [Export] public float Duration = 0.5f;
    [Export] public float Height = 80f;
    [Export] public PackedScene ProjectileScene;
    [Export] public Node2D Visuals;
    [Export] public Node2D ProjectileOrigin;
    [Export] public Sprite2D Sprite;
    [Export] public SquashAndStretch JumpSquashAndStretch;
    [Export] public SquashAndStretch LandSquashAndStretch;

    private Slime _slime;
    private Vector2 _target;
    private float _jumpTimer;

    public override void _Ready() {
        _slime = GetParent().GetParent<Slime>();
    }

    public override void Enter() {
        if (Player.AlivePlayers.Count == 0) {
            GoToState("Idle");

            return;
        }

        _jumpTimer = 0f;

        _target = _slime.GetWeightedTargets()[0].Player.GlobalPosition;

        Sprite.Scale = new Vector2(_target.X > _slime.GlobalPosition.X ? 1f : -1f, 1f);

        JumpSquashAndStretch.Activate();
    }

    public override void PhsysicsUpdate(float delta) {
        _slime.Velocity = (_target - _slime.GlobalPosition).Normalized() * Speed;

        _slime.MoveAndSlide();
    }

    public override void Update(float delta) {
        _jumpTimer += delta;

        float height = Mathf.Pow(Mathf.Sin(_jumpTimer / Duration * Mathf.Pi), 0.75f) * Height;

        Visuals.Position = Vector2.Up * height;

        if (_jumpTimer < Duration) return;

        Projectile projectile = ProjectileScene.Instantiate<Projectile>();

        projectile.Source = _slime;

        _slime.GetParent().AddChild(projectile);

        projectile.GlobalPosition = ProjectileOrigin.GlobalPosition;

        LandSquashAndStretch.Activate();

        GoToState("Idle");
    }

    public override void Exit() {
        Visuals.Position = Vector2.Zero;
    }
}
