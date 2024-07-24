using System;
using Godot;
using Networking;
using Riptide;

public class Idle : EnemyState {

    public Vector2 Interval = new Vector2(0.8f, 1.2f);
    public string AttackState = "attack";

    public Action<float> Movement = null;

    private float _idleTimer = 0;
    private RandomNumberGenerator _randomNumberGenerator = new RandomNumberGenerator();

    public Idle(string name, Enemy enemy) : base(name, enemy) { }

    public override void Initialize() {
        _enemy.NetworkPoint.Register(nameof(AttackRpc), AttackRpc);

        if (Movement == null) Movement = delta => {
            _enemy.Velocity = _enemy.Knockback;

            _enemy.MoveAndSlide();
        };
    }

    public override void Enter() {
        _idleTimer = _randomNumberGenerator.RandfRange(Interval.X, Interval.Y);
    }

    public override void Update(float delta) {
        if (!_enemy.Hurt) _enemy.AnimationPlayer.Play("idle");

        if (!NetworkManager.IsHost) return;

        if (!_enemy.Activated) return;

        _idleTimer -= delta;

        if (_idleTimer > 0) return;

        _enemy.NetworkPoint.SendRpcToClientsFast(nameof(AttackRpc));
    }

    public override void PhsysicsUpdate(float delta) {
        Movement(delta);
    }

    public override void Exit() {
        _enemy.AnimationPlayer.Stop();
    }

    private void AttackRpc(Message message) {
        GoToState(AttackState);
    }
}
