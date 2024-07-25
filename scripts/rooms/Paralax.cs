using Godot;
using System;

public partial class Paralax : Node2D {
	[Export] public float Factor = 0.25f;
	[Export] public float MaxDistance = 100f;

	private Node2D _child;

	public override void _Ready() {
		_child = GetChild<Node2D>(0);
	}

	public override void _Process(double delta) {
		_child.GlobalPosition = GlobalPosition + (GlobalPosition - Camera.Me.GlobalPosition) * Factor;

		if (_child.Position.Length() > MaxDistance) _child.Position = _child.Position.Normalized() * MaxDistance;
	}
}
