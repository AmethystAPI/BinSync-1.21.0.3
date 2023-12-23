using Godot;
using System;

public partial class Projectile : Node2D
{
	private Area2D _damageArea;

	public override void _Ready()
	{
		_damageArea = GetNode<Area2D>("DamageArea");
	}

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += GlobalTransform.BasisXform(Vector2.Right) * 100f * (float)delta;

		foreach (Node2D body in _damageArea.GetOverlappingBodies())
		{
			if (!(body is Entity)) return;

			if (GetMultiplayerAuthority() == Multiplayer.GetUniqueId())
			{
				(body as Entity).Damage(this);
			}

			QueueFree();

			break;
		}
	}
}
