using Godot;
using Networking;
using Riptide;
using System.Collections.Generic;
using System.Linq;

public partial class WorldGenerator : Node2D, NetworkPointUser
{
	private static WorldGenerator s_Me;

	[Export] public PackedScene SpawnRoomScene;
	[Export] public PackedScene RoomScene;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private RandomNumberGenerator _randomNumberGenerator;
	private Room _currentRoom;
	private Room _lastRoom;

	public override void _Ready()
	{
		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(PlaceNextRoomRpc), PlaceNextRoomRpc);
		NetworkPoint.Register(nameof(DespawnLastRoomRpc), DespawnLastRoomRpc);

		s_Me = this;
	}

	public void Start()
	{
		_randomNumberGenerator = new RandomNumberGenerator
		{
			Seed = (ulong)Game.Seed
		};

		SpawnRoom spawnRoom = NetworkManager.SpawnNetworkSafe<SpawnRoom>(SpawnRoomScene, "SpawnRoom");

		AddChild(spawnRoom);

		_currentRoom = spawnRoom;

		_currentRoom.PlaceExit(Vector2.Up);

		_currentRoom.Place();
	}

	public void Cleanup()
	{
		if (_lastRoom != null && IsInstanceValid(_lastRoom))
		{
			_lastRoom.QueueFree();
			_lastRoom = null;
		}

		_currentRoom.QueueFree();
		_currentRoom = null;
	}

	public static void PlaceNextRoom(Vector2 connectionPosition, Vector2 direction)
	{
		if (!NetworkManager.IsHost) return;

		s_Me.NetworkPoint.SendRpcToClients(nameof(PlaceNextRoomRpc), message =>
		{
			message.AddFloat(connectionPosition.X);
			message.AddFloat(connectionPosition.Y);

			message.AddFloat(direction.X);
			message.AddFloat(direction.Y);
		});
	}

	public static void DespawnLastRoom()
	{
		if (!NetworkManager.IsHost) return;

		s_Me.NetworkPoint.SendRpcToClients(nameof(DespawnLastRoomRpc));
	}

	private void PlaceNextRoomRpc(Message message)
	{
		Vector2 connectionPosition = new Vector2(message.GetFloat(), message.GetFloat());
		Vector2 direction = new Vector2(message.GetFloat(), message.GetFloat());

		_lastRoom = _currentRoom;

		Room room = NetworkManager.SpawnNetworkSafe<Room>(RoomScene, "Room");

		AddChild(room);

		_currentRoom = room;

		room.GlobalPosition = connectionPosition;

		Vector2 roomConnectionPosition = room.Entrances[room.EntranceDirections.ToList().IndexOf(-direction)].Position;

		room.GlobalPosition -= roomConnectionPosition;

		room.PlaceEntrance(-direction);

		List<Vector2> possibleExitDirections = new List<Vector2>() { Vector2.Left, Vector2.Up, Vector2.Right };
		if (possibleExitDirections.Contains(-direction)) possibleExitDirections.Remove(-direction);

		Vector2 exitDirection = possibleExitDirections[_randomNumberGenerator.RandiRange(0, possibleExitDirections.Count - 1)];

		room.PlaceExit(exitDirection);

		room.Place();
	}

	private void DespawnLastRoomRpc(Message message)
	{
		if (_lastRoom == null) return;

		_lastRoom.QueueFree();
	}
}
