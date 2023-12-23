using Godot;
using System;

public partial class Projectile : Node2D
{
	[Export] public float Speed = 200f;
	[Export] public float Lifetime = 5f;
	[Export] public float Knockback = 1f;

	public Node2D Source;
	public Vector2 InheritedVelocity;

	private Area2D _damageArea;
	private float _lifetimeTimer;

	public override void _Ready()
	{
		_damageArea = GetNode<Area2D>("DamageArea");

		_lifetimeTimer = Lifetime;
	}

	public override void _Process(double delta)
	{
		_lifetimeTimer -= (float)delta;

		if (_lifetimeTimer > 0) return;

		QueueFree();
	}

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += GlobalTransform.BasisXform(Vector2.Right) * Speed * (float)delta + InheritedVelocity * (float)delta;

		foreach (Node2D body in _damageArea.GetOverlappingBodies())
		{
			if (body is TileMap)
			{
				QueueFree();

				return;
			}

			if (!(body is Damageable)) continue;

			Damageable damageable = body as Damageable;

			if (!damageable.CanDamage(this)) continue;

			damageable.Damage(this);

			QueueFree();

			break;
		}
	}
}
