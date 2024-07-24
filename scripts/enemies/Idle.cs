using Godot;
using Networking;
using Riptide;

public class Idle : State {
    public static Vector2 DefaultInterval = new Vector2(0.8f, 1.2f);

    public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

    private Enemy _enemy;

    private Vector2 _interval;
    private string _attackState;

    private float _idleTimer = 0;
    private RandomNumberGenerator _randomNumberGenerator = new RandomNumberGenerator();
    private float _lastIdleTime;

    public Idle(string name, Enemy enemy, string attackState, Vector2 interval) : base(name) {
        _enemy = enemy;
        _interval = interval;
        _attackState = attackState;
    }

    public override void Initialize() {
        NetworkPoint.Register(nameof(AttackRpc), AttackRpc);
    }

    public override void Enter() {
        _enemy.AnimationPlayer.Play("idle");

        if (_idleTimer > 0) {
            _idleTimer -= (Time.GetTicksMsec() - _lastIdleTime) / 100f;

            return;
        }

        _idleTimer = _randomNumberGenerator.RandfRange(_interval.X, _interval.Y);
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
        GoToState(_attackState);
    }
}
