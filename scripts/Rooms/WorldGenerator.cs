using Godot;
using Networking;
using Riptide;
using System.Collections.Generic;
using System.Linq;

public partial class WorldGenerator : Node2D, NetworkPointUser {
	private static WorldGenerator s_Me;

	[Export] public RoomPlacer SpawnRoomPlacer;
	[Export] public RoomPlacer[] RoomPlacers;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private RandomNumberGenerator _randomNumberGenerator;
	private Room _currentRoom;
	private Room _lastRoom;

	public override void _Ready() {
		s_Me = this;

		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(PlaceNextRoomRpc), PlaceNextRoomRpc);
	}

	public void Start() {
		_randomNumberGenerator = new RandomNumberGenerator {
			Seed = Game.Seed
		};

		SpawnRoom spawnRoom = NetworkManager.SpawnNetworkSafe<SpawnRoom>(SpawnRoomPlacer.RoomScene, "SpawnRoom");

		AddChild(spawnRoom);

		_currentRoom = spawnRoom;

		_currentRoom.Place();

		_currentRoom.PlaceExit(Vector2.Up);
	}

	public void Cleanup() {
		foreach (Node node in GetTree().GetNodesInGroup("Rooms")) {
			node.QueueFree();
		}

		_currentRoom = null;
	}

	public static void PlaceNextRoom(Vector2 connectionPosition, Vector2 direction) {
		if (!NetworkManager.IsHost) return;

		s_Me.NetworkPoint.SendRpcToClients(nameof(PlaceNextRoomRpc), message => {
			message.AddFloat(connectionPosition.X);
			message.AddFloat(connectionPosition.Y);

			message.AddFloat(direction.X);
			message.AddFloat(direction.Y);

			RoomPlacer[] validRoomPlacers = s_Me.RoomPlacers.Where(placer => placer.CanConnectTo(direction)).ToArray();

			RoomPlacer roomPlacer = validRoomPlacers[s_Me._randomNumberGenerator.RandiRange(0, validRoomPlacers.Length - 1)];

			message.AddString(roomPlacer.ResourcePath);

			List<Vector2> possibleExitDirections = roomPlacer.GetDirections();

			if (possibleExitDirections.Contains(-direction)) possibleExitDirections.Remove(-direction);

			GD.Print(roomPlacer.ResourcePath);

			Vector2 exitDirection = possibleExitDirections[s_Me._randomNumberGenerator.RandiRange(0, possibleExitDirections.Count - 1)];

			message.AddFloat(exitDirection.X);
			message.AddFloat(exitDirection.Y);
		});
	}

	private void PlaceNextRoomRpc(Message message) {
		Vector2 connectionPosition = new Vector2(message.GetFloat(), message.GetFloat());
		Vector2 direction = new Vector2(message.GetFloat(), message.GetFloat());

		string roomPlacerPath = message.GetString();
		RoomPlacer roomPlacer = ResourceLoader.Load<RoomPlacer>(roomPlacerPath);

		Room room = NetworkManager.SpawnNetworkSafe<Room>(roomPlacer.RoomScene, "Room");

		AddChild(room);

		_currentRoom = room;

		room.GlobalPosition = connectionPosition;

		Vector2 roomConnectionPosition = room.Connections[room.ConnectionDirections.ToList().IndexOf(-direction)].Position;

		room.GlobalPosition -= roomConnectionPosition;

		room.PlaceEntrance(-direction);

		Vector2 exitDirection = new Vector2(message.GetFloat(), message.GetFloat());

		room.PlaceExit(exitDirection);

		room.Place();
	}
}
