using Godot;
using Networking;
using System;
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
		s_Me._lastRoom = s_Me._currentRoom;

		Room room = NetworkManager.SpawnNetworkSafe<Room>(s_Me.RoomScene, "Room");

		s_Me.AddChild(room);

		s_Me._currentRoom = room;

		room.GlobalPosition = connectionPosition;

		Vector2 roomConnectionPosition = room.Entrances[room.EntranceDirections.ToList().IndexOf(-direction)].Position;

		room.GlobalPosition -= roomConnectionPosition;

		room.PlaceEntrance(-direction);

		List<Vector2> possibleExitDirections = new List<Vector2>() { Vector2.Left, Vector2.Up, Vector2.Right };
		if (possibleExitDirections.Contains(-direction)) possibleExitDirections.Remove(-direction);

		Vector2 exitDirection = possibleExitDirections[s_Me._randomNumberGenerator.RandiRange(0, possibleExitDirections.Count - 1)];

		room.PlaceExit(exitDirection);

		room.Place();
	}

	public static void DespawnLastRoom()
	{
		s_Me._lastRoom.QueueFree();
	}
}
