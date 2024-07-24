using Godot;
using Networking;
using Riptide;

public partial class CrowScared : NodeState, NetworkPointUser {
    [Export] public float Speed = 10f;
    [Export] public float InverseInertia = 4f;
    [Export] public float FlockOffsetRange = 24f;
    [Export] public Vector2 ScaredInterval = new Vector2(1f, 2f);
    [Export] public AnimationPlayer AnimationPlayer;
    [Export] public Sprite2D Sprite;
    [Export] public SquashAndStretch EnterSquashAndStretch;

    public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

    private Crow _crow;
    private Vector2 _flockOffset;
    private RandomNumberGenerator _randomNumberGenerator = new RandomNumberGenerator();
    private float _scaredTimer = 0;

    public override void _Ready() {
        _crow = GetParent().GetParent<Crow>();

        NetworkPoint.Setup(this);

        NetworkPoint.Register(nameof(AttackRpc), AttackRpc);
    }

    public override void Enter() {
        AnimationPlayer.Play("flap");

        _scaredTimer = _randomNumberGenerator.RandfRange(ScaredInterval.X, ScaredInterval.Y);

        PickFlockOffset();

        EnterSquashAndStretch.Activate();
    }

    public override void PhsysicsUpdate(float delta) {
        if (!_crow.NetworkPoint.IsOwner) return;

        if (!_crow.Activated) return;

        Vector2 target = Vector2.Zero;

        foreach (Crow crow in Crow.Crows) {
            target += crow.GlobalPosition;
        }

        target /= Crow.Crows.Count;

        target += _flockOffset;

        _crow.Velocity = _crow.Velocity.Slerp((target - _crow.GlobalPosition).Normalized() * Speed, InverseInertia * delta);

        Sprite.Scale = new Vector2(target.X >= _crow.GlobalPosition.X ? 1f : -1f, 1f);

        _crow.MoveAndSlide();

        if (_crow.GlobalPosition.DistanceTo(target) <= 1) PickFlockOffset();

        _scaredTimer -= delta;

        if (_scaredTimer <= 0) NetworkPoint.BounceRpcToClients(nameof(AttackRpc));
    }

    private void PickFlockOffset() {
        _flockOffset = (Vector2.Right * _randomNumberGenerator.RandfRange(0, FlockOffsetRange)).Rotated(_randomNumberGenerator.RandfRange(0, Mathf.Pi * 2f));
    }

    private void AttackRpc(Message message) {
        GoToState("Aggressive");
    }
}
