using System.Linq;
using Godot;
using Riptide;

public partial class SpawnRoom : Room
{
	public override void PlaceExit(Vector2 direction)
	{
		AddChild(Exits[ExitDirections.ToList().IndexOf(direction)]);
	}

	protected override void Start()
	{
		if (!Game.IsHost()) return;

		Game.SendRpcToClients(this, nameof(EndRpc), MessageSendMode.Reliable, message => { });
	}
}
