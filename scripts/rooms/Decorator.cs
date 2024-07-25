using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class Decorator : Node2D {
	[Export] public DecorationPool DecorationPool;

	private bool _justPressedGenerate = false;
	private List<Vector2> _placed = new List<Vector2>();
	private Node2D _holder;

	public override void _Ready() {
		_holder = GetNode<Node2D>("Holder");

		Generate();
	}

	public override void _Process(double delta) {
#if TOOLS
		if (Input.IsKeyPressed(Key.Backslash) && _justPressedGenerate) return;

		if (!Input.IsKeyPressed(Key.Backslash)) {
			_justPressedGenerate = false;

			return;
		}

		_justPressedGenerate = true;

		Generate();
#endif
	}

	private void Generate() {
		foreach (Node child in _holder.GetChildren()) {
			child.QueueFree();
		}

		_placed.Clear();

		RandomNumberGenerator random = new();

		CollisionShape2D areaNode = GetNode<CollisionShape2D>("Shape");
		RectangleShape2D shape = areaNode.Shape as RectangleShape2D;

		float area = shape.Size.X * shape.Size.Y;
		int amount = (int)MathF.Floor(area / (DecorationPool.Spacing * DecorationPool.Spacing));

		for (int index = 0; index < amount; index++) {
			PackedScene scene = DecorationPool.Scenes[random.RandiRange(0, DecorationPool.Scenes.Length - 1)];

			Vector2 position = areaNode.GlobalPosition + Vector2.Right * random.RandfRange(-shape.Size.X / 2, shape.Size.X / 2) + Vector2.Up * random.RandfRange(-shape.Size.Y / 2, shape.Size.Y / 2); ;

			if (_placed.Where(otherPosition => otherPosition.DistanceTo(position) < DecorationPool.MinimumSpacing).Count() > 0) continue;

			Node2D decoration = scene.Instantiate<Node2D>();
			_holder.AddChild(decoration);

			_placed.Add(position);

			decoration.GlobalPosition = position;

			decoration.Owner = GetOwner();

			if (DecorationPool.Rotate) decoration.GlobalRotation = Mathf.Pi / 2 * random.RandiRange(0, 3);
		}
	}
}
