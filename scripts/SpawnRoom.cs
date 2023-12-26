using System.Linq;
using Godot;

public partial class SpawnRoom : Room
{
	public override void PlaceExit(Vector2 direction)
	{
		AddChild(Exits[ExitDirections.ToList().IndexOf(direction)]);
	}

	protected override void Start()
	{
		CallDeferred(nameof(End));
	}
}
