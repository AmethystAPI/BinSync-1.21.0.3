using Godot;

public partial class Entrance : Node2D
{
	private Node2D _visuals;

	public override void _Ready()
	{
		_visuals = GetNode<Node2D>("Visuals");

		RemoveChild(_visuals);

		GetParent().GetParent<Room>().Started += () =>
		{
			if (!IsInstanceValid(this)) return;

			AddChild(_visuals);
		};
	}
}
