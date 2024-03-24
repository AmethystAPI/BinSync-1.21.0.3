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

	private List<Room> _rooms = new List<Room>();

	public override void _Ready() {
		s_Me = this;

		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(PlaceRoomRpc), PlaceRoomRpc);
	}

	public void Start() {
		_rooms = new List<Room>();

		PlaceSpawnRoom();

		if (!NetworkManager.IsHost) return;

		PlaceRoom();
	}

	public void Cleanup() {
		foreach (Node node in GetTree().GetNodesInGroup("Rooms")) {
			node.QueueFree();
		}
	}

	private void PlaceSpawnRoom() {
		SpawnRoom spawnRoom = NetworkManager.SpawnNetworkSafe<SpawnRoom>(SpawnRoomPlacer.RoomScene, "SpawnRoom");

		AddChild(spawnRoom);

		_rooms.Add(spawnRoom);

		spawnRoom.Place();
	}

	public static void PlaceRoom() {
		if (!NetworkManager.IsHost) return;

		Room _lastRoom = s_Me._rooms[s_Me._rooms.Count - 1];

		RoomPlacer[] validRoomPlacers = s_Me.RoomPlacers.Where(placer => {
			if (!placer.CanConnectTo(_lastRoom.ExitDirection)) return false;

			if (_lastRoom.ExitDirection != Vector2.Up && placer.CanConnectTo(Vector2.Up) && placer.GetDirections().Count == 2) return false;

			return true;
		}).ToArray();

		RoomPlacer roomPlacer = validRoomPlacers[Game.RandomNumberGenerator.RandiRange(0, validRoomPlacers.Length - 1)];

		s_Me.NetworkPoint.SendRpcToClients(nameof(PlaceRoomRpc), message => {
			message.AddString(roomPlacer.ResourcePath);
		});
	}

	private void PlaceRoomRpc(Message message) {
		string roomPlacerPath = message.GetString();
		RoomPlacer roomPlacer = ResourceLoader.Load<RoomPlacer>(roomPlacerPath);

		Room room = NetworkManager.SpawnNetworkSafe<Room>(roomPlacer.RoomScene, "Room");

		AddChild(room);

		Room _lastRoom = _rooms[_rooms.Count - 1];

		_rooms.Add(room);

		room.GlobalPosition = _lastRoom.Exit.GlobalPosition - room.Entrance.GlobalPosition;

		room.Place();

		if (_rooms.Count == 2) {
			room.Activate();
		} else {
			Game.NextRooms.Add(room);
		}
	}
}
