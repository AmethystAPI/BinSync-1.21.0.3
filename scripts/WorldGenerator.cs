using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class WorldGenerator : Node2D
{
	private static WorldGenerator s_Me;

	[Export] public PackedScene SpawnRoomScene;
	[Export] public PackedScene RoomScene;

	private RandomNumberGenerator _randomNumberGenerator;
	private Room _currentRoom;
	private Room _lastRoom;

	public override void _Ready()
	{
		s_Me = this;
	}

	public void Start()
	{
		_randomNumberGenerator = new RandomNumberGenerator
		{
			Seed = (ulong)Game.Me.Seed
		};

		SpawnRoom spawnRoom = SpawnRoomScene.Instantiate<SpawnRoom>();

		AddChild(spawnRoom);

		_currentRoom = spawnRoom;

		_currentRoom.PlaceExit(Vector2.Up);
	}

	public static void PlaceNextRoom(Vector2 connectionPosition, Vector2 direction)
	{
		s_Me._lastRoom = s_Me._currentRoom;

		Room room = s_Me.RoomScene.Instantiate<Room>();

		s_Me.AddChild(room);

		s_Me._currentRoom = room;

		room.GlobalPosition = connectionPosition;

		Vector2 roomConnectionPosition = room.Entrances[room.EntranceDirections.ToList().IndexOf(-direction)].Position;

		room.GlobalPosition -= roomConnectionPosition;

		room.PlaceEntrance(-direction);

		// List<Vector2> possibleExitDirections = new List<Vector2>() { Vector2.Left, Vector2.Up, Vector2.Right };
		List<Vector2> possibleExitDirections = new List<Vector2>() { Vector2.Up };
		if (possibleExitDirections.Contains(-direction)) possibleExitDirections.Remove(-direction);

		Vector2 exitDirection = possibleExitDirections[s_Me._randomNumberGenerator.RandiRange(0, possibleExitDirections.Count - 1)];

		room.PlaceExit(exitDirection);
	}

	public static void DespawnLastRoom()
	{
		s_Me._lastRoom.QueueFree();
	}
}
