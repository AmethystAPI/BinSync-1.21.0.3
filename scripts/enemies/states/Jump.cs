using System;
using Godot;

public class Jump : EnemyState {
    public float Speed = 30f;
    public float Duration = 0.75f;
    public float Height = 16f;
    public string ReturnState = "idle";

    public Action Land;

    private Vector2 _target;
    private float _jumpTimer;

    public Jump(string name, Enemy enemy) : base(name, enemy) { }

    public override void Enter() {
        if (Player.AlivePlayers.Count == 0) {
            GoToState(ReturnState);

            return;
        }

        _jumpTimer = 0f;

        _target = _enemy.GetWeightedTargets()[0].Player.GlobalPosition;

        _enemy.Face(_target);
    }

    public override void Update(float delta) {
        if (!_enemy.Hurt) _enemy.AnimationPlayer.Play("idle");

        _jumpTimer += delta;

        float height = Mathf.Pow(Mathf.Sin(_jumpTimer / Duration * Mathf.Pi), 0.75f) * Height;

        _enemy.VerticalTransform.Position = Vector2.Up * height;

        if (_jumpTimer < Duration) return;

        Land();

        GoToState(ReturnState);
    }

    public override void PhsysicsUpdate(float delta) {
        _enemy.Velocity = (_target - _enemy.GlobalPosition).Normalized() * Speed + _enemy.Knockback;

        _enemy.MoveAndSlide();
    }

    public override void Exit() {
        _enemy.VerticalTransform.Position = Vector2.Zero;
    }
}
