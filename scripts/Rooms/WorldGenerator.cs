using Godot;
using Networking;
using Riptide;
using System.Collections.Generic;
using System.Data;
using System.Linq;

public partial class WorldGenerator : Node2D, NetworkPointUser {
	private static WorldGenerator s_Me;

	[Export] public Biome[] Biomes;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private TileMapLayer _wallsTileMapLayer;

	public override void _Ready() {
		s_Me = this;

		NetworkPoint.Setup(this);

		_wallsTileMapLayer = GetNode<TileMapLayer>("Walls");
	}

	public void Start() {
		RoomLayout.Connection lastConnection;

		RoomLayout spawnRoomLayout = Biomes[0].GetSpawnRoomLayout(0);

		Vector2 spawnRoomPlaceLocation = -((spawnRoomLayout.BottomRightBound - spawnRoomLayout.TopLeftBound) / 2f).Floor();
		lastConnection = spawnRoomLayout.GetConnections()[0];
		lastConnection.Location += spawnRoomPlaceLocation;

		PlaceRoomLayout(spawnRoomLayout, (Vector2I)spawnRoomPlaceLocation);

		for (int index = 0; index < 5; index++) {
			RoomLayout roomLayout = Biomes[0].GetRoomLayout(Game.RandomNumberGenerator.RandiRange(0, Biomes[0].Rooms.Length - 1));

			List<RoomLayout.Connection> connections = roomLayout.GetConnections().ToList();
			RoomLayout.Connection targetConnection = connections.Where(connection => connection.Direction == new Vector2(-lastConnection.Direction.X, -lastConnection.Direction.Y)).First();
			connections.Remove(targetConnection);

			Vector2 placeLocation = lastConnection.Location - targetConnection.Location;

			PlaceRoomLayout(roomLayout, new Vector2I((int)placeLocation.X, (int)placeLocation.Y));

			lastConnection = connections[0];
			lastConnection.Location += placeLocation;
		}
	}

	private void PlaceRoomLayout(RoomLayout roomLayout, Vector2I location) {
		foreach (Vector2 tileLocation in roomLayout.Walls) {
			Vector2I realTileLocation = location + new Vector2I((int)tileLocation.X, (int)tileLocation.Y) - new Vector2I((int)roomLayout.TopLeftBound.X, (int)roomLayout.TopLeftBound.Y);

			_wallsTileMapLayer.SetCell(realTileLocation, 0, new Vector2I(3, 0));
		}
	}
}
