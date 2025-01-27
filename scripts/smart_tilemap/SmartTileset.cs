using Godot;

[Tool]
public partial class SmartTileset {
    public SmartTile[] Tiles;
    public TileSet TileSet;

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