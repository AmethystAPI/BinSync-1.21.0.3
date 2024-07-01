using Godot;
using Networking;
using Riptide;

public partial class RangedApproach : State, NetworkPointUser {
    [Export] public Vector2 IdleInterval = new Vector2(0.8f, 1.2f);
    [Export] public float Speed = 30;
    [Export] public float TargetDistance = 64;
    [Export] public string AttackState = "Attack";
    [Export] public string RunAnimation = "run";
    [Export] public string IdleAnimation = "idle";
    [Export] public AnimationPlayer AnimationPlayer;

    public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

    private Enemy _enemy;
    private float _idleTimer = 0;
    private RandomNumberGenerator _randomNumberGenerator = new RandomNumberGenerator();
    private float _lastIdleTime;

    public override void _Ready() {
        _enemy = GetParent().GetParent<Enemy>();

        NetworkPoint.Setup(this);

        NetworkPoint.Register(nameof(AttackRpc), AttackRpc);
    }

    public override void Enter() {
        // AnimationPlayer.Play(RunAnimation);

        if (_idleTimer > 0) {
            _idleTimer -= (Time.GetTicksMsec() - _lastIdleTime) / 100f;

            return;
        }

        _idleTimer = _randomNumberGenerator.RandfRange(IdleInterval.X, IdleInterval.Y);
    }

    public override void Update(float delta) {
        if (!NetworkManager.IsHost) return;

        if (!_enemy.Activated) return;

        _idleTimer -= delta;

        if (_idleTimer > 0) return;

        NetworkPoint.SendRpcToClientsFast(nameof(AttackRpc));
    }

    public override void PhsysicsUpdate(float delta) {
        if (!_enemy.Activated) return;

        Vector2 target = _enemy.GetWeightedTargets()[0].Player.GlobalPosition;

        _enemy.Velocity = (target - _enemy.GlobalPosition).Normalized() * Speed * (target.DistanceTo(_enemy.GlobalPosition) > TargetDistance ? 1 : -1);

        _enemy.MoveAndSlide();
    }

    public override void Exit() {
        _lastIdleTime = Time.GetTicksMsec();
    }

    private void AttackRpc(Message message) {
        GoToState(AttackState);
    }
}
