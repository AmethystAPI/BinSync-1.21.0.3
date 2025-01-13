using Godot;

public class Dead : EnemyState {
    private float _deadTimer;

    public Dead(string name, Enemy enemy) : base(name, enemy) { }

    public override void Enter() {
        _enemy.Velocity = _enemy.Knockback / 5f;

        Delay.Execute(0.5f, Die);
    }

    public override void Update(float delta) {
        _enemy.Face(_enemy.Knockback.X <= 1);

        _deadTimer += delta;

        _enemy.VerticalTransform.Position = Vector2.Up * Mathf.Sin(_deadTimer * Mathf.Pi / 0.5f) * 16f;
    }

    public override void PhsysicsUpdate(float delta) {
        _enemy.MoveAndSlide();
    }

    private void Die() {
        Audio.Play("enemy_die");

        Node2D deathParticle = _enemy.DeathParticle.Instantiate<Node2D>();
        _enemy.GetParent().AddChild(deathParticle);
        deathParticle.GlobalPosition = _enemy.GlobalPosition;

        _enemy.QueueFree();
    }
}
