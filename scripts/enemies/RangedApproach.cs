using Godot;
using Networking;
using Riptide;

public partial class RangedApproach : State, NetworkPointUser {
    [Export] public Vector2 IdleInterval = new Vector2(0.8f, 1.2f);
    [Export] public float Speed = 30;
    [Export] public float TargetDistance = 64;
    [Export] public float RestRange = 8;
    [Export] public string AttackState = "Attack";
    [Export] public string RunAnimation = "run";
    [Export] public string IdleAnimation = "idle";
    [Export] public AnimationPlayer AnimationPlayer;

    public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

    private Enemy _enemy;
    private float _idleTimer = 0;
    private RandomNumberGenerator _randomNumberGenerator = new RandomNumberGenerator();
    private float _lastIdleTime;
    private bool _resting = false;

    public override void _Ready() {
        _enemy = GetParent().GetParent<Enemy>();

        NetworkPoint.Setup(this);

        NetworkPoint.Register(nameof(AttackRpc), AttackRpc);
    }

    public override void Enter() {
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
        float distance = target.DistanceTo(_enemy.GlobalPosition);
        Vector2 direction = (target - _enemy.GlobalPosition).Normalized();

        if (_resting) {
            if (Mathf.Abs(distance - TargetDistance) <= RestRange) {
                _enemy.Velocity = Vector2.Zero;

                _enemy.MoveAndSlide();

                AnimationPlayer.Play(IdleAnimation);

                return;
            }

            _resting = false;
        }

        if (Mathf.Abs(distance - TargetDistance) <= 1f) {
            _resting = true;

            _enemy.Velocity = Vector2.Zero;

            _enemy.MoveAndSlide();

            AnimationPlayer.Play(IdleAnimation);

            return;
        }

        _enemy.Velocity = direction * Speed * (distance > TargetDistance ? 1 : -1);

        AnimationPlayer.Play(RunAnimation);

        _enemy.MoveAndSlide();
    }

    public override void Exit() {
        _lastIdleTime = Time.GetTicksMsec();
    }

    private void AttackRpc(Message message) {
        GoToState(AttackState);
    }
}
