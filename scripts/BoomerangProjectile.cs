using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BoomerangProjectile : Projectile {
	[Export] public float TargetBounceRange = 48f;
	[Export] public float MinimumVelocity = 4f;
	[Export] public int MaximumTargetBounces = 2;
	[Export] public float CollectDistance = 8f;

	private bool _returningToPlayer;
	private new Vector2 _velocity;
	private int _bounces = 0;

	public override void _Ready() {
		base._Ready();

		_velocity = GlobalTransform.BasisXform(Vector2.Right) * Speed;
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if (_returningToPlayer && IsInstanceValid(Source) && Source.GlobalPosition.DistanceTo(GlobalPosition) <= CollectDistance) QueueFree();

		GlobalRotation = Vector2.Zero.AngleToPoint(_velocity);
	}

	public override void Movement(float delta) {
		bool sourceValid = IsInstanceValid(Source);

		if (_returningToPlayer && sourceValid) {
			_velocity = _velocity.Lerp((Source.GlobalPosition - GlobalPosition).Normalized() * Speed, Resistance * (float)delta);
		} else {
			_velocity = _velocity.Lerp(Vector2.Zero, Resistance * (float)delta);
		}

		bool followSourceVelocity = sourceValid && Source is CharacterBody2D && _bounces == 0;

		if (followSourceVelocity) {
			GlobalPosition += _velocity * (float)delta + ((CharacterBody2D)Source).Velocity * (float)delta;
		} else {
			GlobalPosition += _velocity * (float)delta;
		}

		if (_velocity.Length() <= MinimumVelocity) _returningToPlayer = true;
	}

	public override void OnHit(Node2D body) {
		_bounces++;

		bool hitTerrain = body is TileMap || body is Barrier;

		if (hitTerrain) {
			if (!_returningToPlayer) Bounce();

			_returningToPlayer = true;

			return;
		}

		List<Enemy> targets = GetTree().GetNodesInGroup("Enemies").ToList().Cast<Enemy>().ToList();

		targets = targets.Where(enemy => enemy != body).Where(enemy => enemy.GlobalPosition.DistanceTo(GlobalPosition) <= TargetBounceRange).ToList();

		if (targets.Count == 0 || _bounces >= MaximumTargetBounces) {
			Bounce();

			return;
		}

		_returningToPlayer = false;

		Enemy closestEnemy = targets.MinBy(enemy => enemy.GlobalPosition.DistanceSquaredTo(GlobalPosition));

		_velocity = Vector2.Right.Rotated(GlobalPosition.AngleToPoint(closestEnemy.GlobalPosition)) * Speed;
	}

	private void Bounce() {
		_velocity = _velocity.Rotated(Mathf.Pi).Normalized() * Speed;
	}
}
