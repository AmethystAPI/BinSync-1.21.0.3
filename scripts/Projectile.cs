using Godot;
using System;

public partial class Projectile : Node2D
{
	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += GlobalTransform.BasisXform(Vector2.Right) * 300f * (float)delta;
	}
}
