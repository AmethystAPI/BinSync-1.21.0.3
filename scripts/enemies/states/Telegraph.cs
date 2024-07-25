using Godot;
using Networking;
using Riptide;

public class Telegraph : EnemyState {
    public float Duration = 0.5f;
    public string Animation = "telgraph_attack";

    private string _attackState = "attack";
    private float _timer = 0;

    public Telegraph(string name, Enemy enemy, string attackState) : base(name, enemy) {
        _attackState = attackState;
    }

    public override void Enter() {
        _timer = Duration;
    }

    public override void Update(float delta) {
        if (!_enemy.Hurt) _enemy.AnimationPlayer.Play(Animation);

        _timer -= delta;

        if (_timer > 0) return;

        GoToState(_attackState);
    }

    public override void PhsysicsUpdate(float delta) {
        _enemy.Velocity = _enemy.Knockback;

        _enemy.MoveAndSlide();
    }

    public override void Exit() {
        _enemy.AnimationPlayer.Stop();
    }
}
