using Godot;
using System;

public partial class Projectile : Node2D
{
	[Export] public float Speed = 100f;
	[Export] public float Lifetime = 5f;

	public Node2D Source;

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
		GlobalPosition += GlobalTransform.BasisXform(Vector2.Right) * Speed * (float)delta;

		foreach (Node2D body in _damageArea.GetOverlappingBodies())
		{
			if (body is TileMap)
			{
				QueueFree();

				return;
			}

			if (!(body is Damageable)) continue;

			if (!(body as Damageable).CanDamage(this)) continue;

			if (GetMultiplayerAuthority() == Multiplayer.GetUniqueId())
			{
				(body as Damageable).Damage(this);
			}

			QueueFree();

			break;
		}
	}
}
