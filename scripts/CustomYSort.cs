using Godot;
using System;

public partial class CustomYSort : Node2D {
	[Export] public float Offset = 16f;

	private Node2D _child;

	public override void _Ready() {
		_child = GetChild<Node2D>(0);
	}

	public override void _Process(double delta) {
		Vector2 childPosition = _child.GlobalPosition;
		float childRotation = _child.GlobalRotation;

		GlobalPosition = _child.GlobalPosition + Vector2.Down * Offset;

		_child.GlobalPosition = childPosition;
		_child.GlobalRotation = childRotation;
	}
}
