using Godot;

public partial class Hurt : State {
    [Export] public string ReturnState = "Idle";
    [Export] public AnimationPlayer AnimationPlayer;
    [Export] public Node2D Visuals;

    public Vector2 Knockback;

    private Enemy _enemy;
    private bool _dead;
    private float _deadTimer;

    public override void _Ready() {
        _enemy = GetParent().GetParent<Enemy>();
    }

    public override void Enter() {
        AnimationPlayer.Play("Hurt");

        if (_enemy.Health > 0) return;

        _dead = true;

        Delay.Execute(0.5f, Die);

        Knockback /= 3f;
    }

    public override void PhsysicsUpdate(float delta) {
        Visuals.Scale = new Vector2(Knockback.X <= 1 ? 1 : -1, 1);

        if (!_dead) Knockback = Knockback.Lerp(Vector2.Zero, (float)delta * 12f);

        _enemy.Velocity = Knockback;

        _enemy.MoveAndSlide();

        if (_dead) {
            _deadTimer += delta;

            Visuals.Position = Vector2.Up * Mathf.Sin(_deadTimer * Mathf.Pi * 2f) * 24f;
        } else if (Knockback.Length() < 3.5f) {
            GoToState(ReturnState);
        }
    }

    public override void Exit() {
        AnimationPlayer.Play(ReturnState);
    }

    private void Die() {
        _enemy.GetParent<Room>().RemoveEnemy();

        _enemy.QueueFree();
    }
}
