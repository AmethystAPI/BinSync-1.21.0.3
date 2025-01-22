using Godot;

[Tool]
public partial class SmartTileset : Resource {
    [Export] public SmartTile[] Tiles;
    [Export] public TileSet TileSet;

    public void Apply(TileMapLayer tileMapLayer) {
        tileMapLayer.TileSet = TileSet;
    }

    public SmartTile GetTile(string id) {
        foreach (SmartTile tile in Tiles) {
            if (tile.Id == id) return tile;
        }

        return null;
    }
}