using Godot;

public class Telegraph : EnemyState {
    public float Duration = 0.5f;
    public string Animation = "telegraph_attack";

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
        Vector2 target = _enemy.GetWeightedTargets()[0].Player.GlobalPosition;
        _enemy.Face(target);

        _enemy.Velocity = _enemy.Knockback;

        _enemy.MoveAndSlide();
    }

    public override void Exit() {
        _enemy.AnimationPlayer.Stop();
    }
}
