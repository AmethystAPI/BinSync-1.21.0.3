using Godot;
using Networking;
using Riptide;
using System.Collections.Generic;
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
		PlaceRoomLayout(Biomes[0].GetRoomLayout(0), Vector2I.Zero);
	}

	private void PlaceRoomLayout(RoomLayout roomLayout, Vector2I location) {
		GD.Print(roomLayout.Walls.Length);

		foreach (Vector2 tileLocation in roomLayout.Walls) {
			Vector2I realTileLocation = location + new Vector2I((int)tileLocation.X, (int)tileLocation.Y) - new Vector2I((int)roomLayout.TopLeftBound.X, (int)roomLayout.TopLeftBound.Y);

			_wallsTileMapLayer.SetCell(realTileLocation, 0, new Vector2I(3, 0));

			GD.Print(realTileLocation);
		}
	}
}
