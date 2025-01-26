using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BoomerangProjectile : Projectile {
	[Export] public float TargetBounceRange = 48f;
	[Export] public float MinimumVelocity = 4f;
	[Export] public int MaximumTargetBounces = 2;
	[Export] public float CollectDistance = 8f;

	public new Vector2 Velocity;

	private bool _returningToPlayer;
	private int _bounces = 0;

	public override void _Ready() {
		base._Ready();

		Velocity = GlobalTransform.BasisXform(Vector2.Right) * Speed;
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if (_returningToPlayer && IsInstanceValid(Source) && Source.GlobalPosition.DistanceTo(GlobalPosition) <= CollectDistance) QueueFree();

		GlobalRotation = Vector2.Zero.AngleToPoint(Velocity);
	}

	public override void Movement(float delta) {
		bool sourceValid = IsInstanceValid(Source);

		if (_returningToPlayer && sourceValid) {
			Velocity = Velocity.Lerp((Source.GlobalPosition - GlobalPosition).Normalized() * Speed, Resistance * (float)delta);
		} else {
			Velocity = Velocity.Lerp(Vector2.Zero, Resistance * (float)delta);
		}

		bool followSourceVelocity = sourceValid && Source is CharacterBody2D && _bounces == 0;

		if (followSourceVelocity) {
			GlobalPosition += Velocity * (float)delta + ((CharacterBody2D)Source).Velocity * (float)delta;
		} else {
			GlobalPosition += Velocity * (float)delta;
		}

		if (Velocity.Length() <= MinimumVelocity) _returningToPlayer = true;
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

		Velocity = Vector2.Right.Rotated(GlobalPosition.AngleToPoint(closestEnemy.GlobalPosition)) * Speed;
	}

	private void Bounce() {
		Velocity = Velocity.Rotated(Mathf.Pi).Normalized() * Speed;
	}
}
