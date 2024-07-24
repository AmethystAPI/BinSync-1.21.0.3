using System;
using Godot;

public class Jump : EnemyState {
    public float Speed = 30f;
    public float Duration = 0.75f;
    public float Height = 16f;
    public string ReturnState = "idle";

    public Action Land;

    public Node2D JumpTransform;

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


    public override void PhsysicsUpdate(float delta) {
        _enemy.Velocity = (_target - _enemy.GlobalPosition).Normalized() * Speed;

        _enemy.MoveAndSlide();
    }

    public override void Update(float delta) {
        _jumpTimer += delta;

        float height = Mathf.Pow(Mathf.Sin(_jumpTimer / Duration * Mathf.Pi), 0.75f) * Height;

        JumpTransform.Position = Vector2.Up * height;

        if (_jumpTimer < Duration) return;

        Land();

        GoToState(ReturnState);
    }

    public override void Exit() {
        JumpTransform.Position = Vector2.Zero;
    }
}
