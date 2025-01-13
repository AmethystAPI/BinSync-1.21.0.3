using Godot;

public class LoadedRoom {
    public WorldGenerator.RoomPlacement RoomPlacement;

    private World _world;
    private Biome _biome;
    private bool _activated = false;

    public LoadedRoom(WorldGenerator.RoomPlacement roomPlacement, World world, Biome biome) {
        RoomPlacement = roomPlacement;
        _world = world;
        _biome = biome;
    }

    public void Load() {
        foreach (Vector2 tileLocation in RoomPlacement.RoomLayout.Walls) {
            Vector2I realTileLocation = RoomPlacement.Location + new Vector2I((int)tileLocation.X, (int)tileLocation.Y);

            _world.WallsTileMapLayer.SetCell(realTileLocation, 0, new Vector2I(3, 0));
        }
    }

    public void Unload() {
        foreach (Vector2 tileLocation in RoomPlacement.RoomLayout.Walls) {
            Vector2I realTileLocation = RoomPlacement.Location + new Vector2I((int)tileLocation.X, (int)tileLocation.Y);

            _world.WallsTileMapLayer.SetCell(realTileLocation, -1);
        }
    }

    public void Update() {
        if (RoomPlacement.Type != WorldGenerator.RoomPlacement.RoomType.None) return;

        if (_activated) return;

        foreach (Player player in Player.AlivePlayers) {
            if (player.GlobalPosition.X < RoomPlacement.GetTopLeftBound().X * 16) continue;
            if (player.GlobalPosition.X > RoomPlacement.GetBottomRightBound().X * 16) continue;
            if (player.GlobalPosition.Y < RoomPlacement.GetTopLeftBound().Y * 16) continue;
            if (player.GlobalPosition.Y > RoomPlacement.GetBottomRightBound().Y * 16) continue;

            Activate();

            break;
        }
    }

    private void Activate() {
        _activated = true;

        GD.Print("Room activated! " + RoomPlacement.Location);
    }
}