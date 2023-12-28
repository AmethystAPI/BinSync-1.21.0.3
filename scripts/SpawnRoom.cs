using System.Linq;
using Godot;
using Networking;
using Riptide;

public partial class SpawnRoom : Room
{
	public override void PlaceExit(Vector2 direction)
	{
		AddChild(Exits[ExitDirections.ToList().IndexOf(direction)]);
	}

	protected override void Start()
	{
		if (!NetworkManager.IsHost) return;

		NetworkPoint.SendRpcToClients(nameof(EndRpc));
	}
}
