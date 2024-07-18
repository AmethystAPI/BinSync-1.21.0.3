using Godot;
using System;

public partial class NonInterruptingHurt : State {
	[Export] public string ReturnState = "Idle";
	[Export] public AnimationPlayer AnimationPlayer;
	[Export] public Node2D Visuals;
	[Export] public PackedScene DeathParticle;

	public Vector2 Knockback;

	private Enemy _enemy;
	private bool _dead;
	private float _deadTimer;
	private float _flashTimer;
	private float _stunTimer;

	public override void _Ready() {
		_enemy = GetParent().GetParent<Enemy>();
	}

	public override void Enter() {
		AnimationPlayer.Play("hurt");

		if (_enemy.Health > 0) return;

		if (_dead) return;

		_dead = true;

		Delay.Execute(0.5f, Die);

		Knockback /= 5f;
	}

	public override void PhsysicsUpdate(float delta) {
		Visuals.Scale = new Vector2(Knockback.X <= 1 ? 1 : -1, 1);

		if (!_dead) Knockback = Knockback.Lerp(Vector2.Zero, (float)delta * 12f);

		_enemy.Velocity = Knockback;

		_enemy.MoveAndSlide();

		if (_dead) {
			_deadTimer += delta;

			Visuals.Position = Vector2.Up * Mathf.Sin(_deadTimer * Mathf.Pi / 0.5f) * 16f;
		} else if (Knockback.Length() < 3.5f) {
			GoToState(ReturnState);
		}
	}

	private void Die() {
		Audio.Play("enemy_die");

		Node2D deathParticle = DeathParticle.Instantiate<Node2D>();
		_enemy.GetParent().AddChild(deathParticle);
		deathParticle.GlobalPosition = _enemy.GlobalPosition;

		_enemy.GetParent<Room>().EnemyDied(_enemy);

		_enemy.QueueFree();
	}
}
