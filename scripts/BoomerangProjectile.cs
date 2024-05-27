using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BoomerangProjectile : Projectile {
	[Export] public float BounceRange = 48f;
	[Export] public float ReturnVelocity = 4f;

	private bool _returningToPlayer;

	public override void _Process(double delta) {
		base._Process(delta);

		if (_returningToPlayer && Source.GlobalPosition.DistanceTo(GlobalPosition) <= 4f) QueueFree();
	}

	public override void Movement(float delta) {
		GlobalPosition += GlobalTransform.BasisXform(Vector2.Right) * _velocity * (float)delta;

		if (_returningToPlayer) {
			_velocity = Mathf.Lerp(_velocity, Speed, Resistance * (float)delta);

			GlobalRotation = GlobalPosition.AngleToPoint(Source.GlobalPosition);
		} else {
			_velocity = Mathf.Lerp(_velocity, 0f, Resistance * (float)delta);
		}

		if (Mathf.Abs(_velocity) > ReturnVelocity) return;

		_returningToPlayer = true;
	}

	public override void OnHit(Node2D body) {
		if (body is TileMap || body is Barrier) {
			_velocity = Speed;

			GlobalRotation = GlobalRotation;

			_returningToPlayer = true;

			return;
		}

		List<Enemy> enemies = GetTree().GetNodesInGroup("Enemies").ToList().Cast<Enemy>().ToList();

		enemies = enemies.Where(enemy => enemy != body).Where(enemy => enemy.GlobalPosition.DistanceTo(GlobalPosition) <= BounceRange).ToList();

		if (enemies.Count == 0) {
			_velocity = Speed;

			GlobalRotation = GlobalRotation + 180f;

			return;
		}

		_returningToPlayer = false;

		Enemy closestEnemy = enemies.MinBy(enemy => enemy.GlobalPosition.DistanceSquaredTo(GlobalPosition));

		_velocity = Speed;

		GlobalRotation = GlobalPosition.AngleToPoint(closestEnemy.GlobalPosition);
	}
}
