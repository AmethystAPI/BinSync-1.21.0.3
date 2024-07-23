using Godot;
using Networking;
using Riptide;

public partial class Idle : NodeState, NetworkPointUser {
    [Export] public Vector2 IdleInterval = new Vector2(0.8f, 1.2f);
    [Export] public string AttackState = "Attack";
    [Export] public string Animation = "idle";
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
        AnimationPlayer.Play(Animation);

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

    public override void Exit() {
        _lastIdleTime = Time.GetTicksMsec();
    }

    private void AttackRpc(Message message) {
        GoToState(AttackState);
    }
}
