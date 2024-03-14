using System.Collections.Generic;
using Godot;

[Tool]
public partial class TileMapTool : Node {
    private bool _justPressedGenerate = false;

    public override void _Process(double delta) {
        if (Input.IsKeyPressed(Key.Backslash) && _justPressedGenerate) return;

        if (!Input.IsKeyPressed(Key.Backslash)) {
            _justPressedGenerate = false;

            return;
        }

        _justPressedGenerate = true;

        Generate(GetParent<TileMap>());
    }

    private void Generate(TileMap tileMap) {
        GD.Print("Generating...");

        List<string> layerNames = new List<string>();

        for (int index = 0; index < tileMap.GetLayersCount(); index++) {
            layerNames.Add(tileMap.GetLayerName(index));
        }

        int wallsLayer = layerNames.IndexOf("Walls");
        int roofsLayer = layerNames.IndexOf("Roofs");
        int shadowLayer = layerNames.IndexOf("Shadow");

        tileMap.ClearLayer(roofsLayer);
        tileMap.ClearLayer(shadowLayer);

        Godot.Collections.Array<Vector2I> wallUsedCells = tileMap.GetUsedCells(wallsLayer);

        foreach (Vector2I position in wallUsedCells) {
            tileMap.SetCell(roofsLayer, position, 0, new Vector2I(1, 4));

            bool right = tileMap.GetCellTileData(wallsLayer, position + Vector2I.Right) != null;
            bool down = tileMap.GetCellTileData(wallsLayer, position + Vector2I.Down) != null;
            bool downRight = tileMap.GetCellTileData(wallsLayer, position + Vector2I.Down + Vector2I.Right) != null;

            if (!right) tileMap.SetCell(roofsLayer, position, 0, new Vector2I(2, 4));
            if (!down) tileMap.SetCell(roofsLayer, position, 0, new Vector2I(1, 5));
            if (!right && !down && !downRight) tileMap.SetCell(roofsLayer, position, 0, new Vector2I(2, 5));
        }
    }
}