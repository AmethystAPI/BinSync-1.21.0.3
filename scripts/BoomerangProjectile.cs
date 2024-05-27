using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BoomerangProjectile : Projectile {
	[Export] public float BounceRange = 48f;
	[Export] public float ReturnVelocity = 4f;

	private bool _returningToPlayer;
	private new Vector2 _velocity;
	private int _bounces = 0;

	public override void _Ready() {
		base._Ready();

		_velocity = GlobalTransform.BasisXform(Vector2.Right) * Speed;
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if (_returningToPlayer && Source.GlobalPosition.DistanceTo(GlobalPosition) <= 8f) QueueFree();

		GlobalRotation = Vector2.Zero.AngleToPoint(_velocity);
	}

	public override void Movement(float delta) {

		if (_returningToPlayer && IsInstanceValid(Source)) {
			_velocity = _velocity.Lerp((Source.GlobalPosition - GlobalPosition).Normalized() * Speed, Resistance * (float)delta);

			GlobalRotation = GlobalPosition.AngleToPoint(Source.GlobalPosition);
		} else {
			_velocity = _velocity.Lerp(Vector2.Zero, Resistance * (float)delta);
		}

		if (IsInstanceValid(Source) && Source is CharacterBody2D characterBody2D && _bounces == 0) {
			GlobalPosition += _velocity * (float)delta + characterBody2D.Velocity * (float)delta;
		} else {
			GlobalPosition += _velocity * (float)delta;
		}

		if (_velocity.Length() > ReturnVelocity) return;

		_returningToPlayer = true;
	}

	public override void OnHit(Node2D body) {
		_bounces++;

		if (body is TileMap || body is Barrier) {
			_returningToPlayer = true;

			return;
		}

		List<Enemy> enemies = GetTree().GetNodesInGroup("Enemies").ToList().Cast<Enemy>().ToList();

		enemies = enemies.Where(enemy => enemy != body).Where(enemy => enemy.GlobalPosition.DistanceTo(GlobalPosition) <= BounceRange).ToList();

		if (enemies.Count == 0 || _bounces >= 2) {
			_velocity = _velocity.Rotated(Mathf.Pi).Normalized() * Speed;

			return;
		}

		_returningToPlayer = false;

		Enemy closestEnemy = enemies.MinBy(enemy => enemy.GlobalPosition.DistanceSquaredTo(GlobalPosition));

		_velocity = Vector2.Right.Rotated(GlobalPosition.AngleToPoint(closestEnemy.GlobalPosition)) * Speed;
	}
}
