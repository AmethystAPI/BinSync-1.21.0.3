using Godot;

public partial class Hurt : State
{
    public Vector2 Knockback;

    private Enemy _enemy;

    public override void _Ready()
    {
        _enemy = GetParent().GetParent<Enemy>();
    }

    public override void Enter()
    {
        if (_enemy.Health > 0) return;

        _enemy.GetParent<Room>().RemoveEnemy();

        _enemy.QueueFree();
    }

    public override void PhsysicsUpdate(float delta)
    {
        Knockback = Knockback.Lerp(Vector2.Zero, (float)delta * 12f);

        _enemy.Velocity = Knockback;

        _enemy.MoveAndSlide();

        if (Knockback.Length() < 3.5f) GoToState("Idle");
    }
}
