using Godot;
using Networking;
using Riptide;

public class Idle : State {

    public Vector2 Interval = new Vector2(0.8f, 1.2f);
    public string AttackState = "attack";

    private Enemy _enemy;

    private float _idleTimer = 0;
    private RandomNumberGenerator _randomNumberGenerator = new RandomNumberGenerator();
    private float _lastIdleTime;

    public Idle(string name, Enemy enemy) : base(name) {
        _enemy = enemy;
    }

    public override void Initialize() {
        _enemy.NetworkPoint.Register(nameof(AttackRpc), AttackRpc);
    }

    public override void Enter() {
        _enemy.AnimationPlayer.Play("idle");

        if (_idleTimer > 0) {
            _idleTimer -= (Time.GetTicksMsec() - _lastIdleTime) / 100f;

            return;
        }

        _idleTimer = _randomNumberGenerator.RandfRange(Interval.X, Interval.Y);
    }

    public override void Update(float delta) {
        if (!NetworkManager.IsHost) return;

        if (!_enemy.Activated) return;

        _idleTimer -= delta;

        if (_idleTimer > 0) return;

        _enemy.NetworkPoint.SendRpcToClientsFast(nameof(AttackRpc));
    }

    public override void Exit() {
        _lastIdleTime = Time.GetTicksMsec();
    }

    private void AttackRpc(Message message) {
        GoToState(AttackState);
    }
}
