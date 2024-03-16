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

        Godot.Collections.Array<Vector2I> wallUsedCells = tileMap.GetUsedCells(wallsLayer);

        tileMap.ClearLayer(roofsLayer);

        foreach (Vector2I position in wallUsedCells) {
            if (tileMap.GetCellTileData(wallsLayer, position + Vector2I.Down) == null) continue;

            tileMap.SetCell(roofsLayer, position, 0, new Vector2I(1, 4));

            bool right = RoofTileAt(wallsLayer, position + Vector2I.Right, tileMap);
            bool left = RoofTileAt(wallsLayer, position + Vector2I.Left, tileMap);
            bool up = RoofTileAt(wallsLayer, position + Vector2I.Up, tileMap);
            bool down = RoofTileAt(wallsLayer, position + Vector2I.Down, tileMap);

            bool upRight = RoofTileAt(wallsLayer, position + Vector2I.Up + Vector2I.Right, tileMap);
            bool upLeft = RoofTileAt(wallsLayer, position + Vector2I.Up + Vector2I.Left, tileMap);
            bool downRight = RoofTileAt(wallsLayer, position + Vector2I.Down + Vector2I.Right, tileMap);
            bool downLeft = RoofTileAt(wallsLayer, position + Vector2I.Down + Vector2I.Left, tileMap);

            if (!right) tileMap.SetCell(roofsLayer, position, 0, new Vector2I(2, 4));
            if (!left) tileMap.SetCell(roofsLayer, position, 0, new Vector2I(0, 4));
            if (!up) tileMap.SetCell(roofsLayer, position, 0, new Vector2I(1, 3));
            if (!down) tileMap.SetCell(roofsLayer, position, 0, new Vector2I(1, 5));

            if (!right && !down && !downRight) tileMap.SetCell(roofsLayer, position, 0, new Vector2I(2, 5));
            if (right && down && !downRight) tileMap.SetCell(roofsLayer, position, 0, new Vector2I(1, 6));

            if (!left && !down && !downLeft) tileMap.SetCell(roofsLayer, position, 0, new Vector2I(0, 5));
            if (left && down && !downLeft) tileMap.SetCell(roofsLayer, position, 0, new Vector2I(2, 6));

            if (!right && !up && !upRight) tileMap.SetCell(roofsLayer, position, 0, new Vector2I(2, 3));
            if (right && up && !upRight) tileMap.SetCell(roofsLayer, position, 0, new Vector2I(1, 7));

            if (!left && !up && !upLeft) tileMap.SetCell(roofsLayer, position, 0, new Vector2I(0, 3));
            if (left && up && !upLeft) tileMap.SetCell(roofsLayer, position, 0, new Vector2I(2, 7));
        }

        foreach (Vector2I position in wallUsedCells) {
            tileMap.SetCell(wallsLayer, position, 0, new Vector2I(3, 0));

            bool right = WallTileAt(wallsLayer, position + Vector2I.Right, tileMap);
            bool left = WallTileAt(wallsLayer, position + Vector2I.Left, tileMap);
            bool down = WallTileAt(wallsLayer, position + Vector2I.Down, tileMap);

            bool downRight = WallTileAt(wallsLayer, position + Vector2I.Down + Vector2I.Right, tileMap);
            bool downLeft = WallTileAt(wallsLayer, position + Vector2I.Down + Vector2I.Left, tileMap);

            bool downDown = WallTileAt(wallsLayer, position + Vector2I.Down * 2, tileMap);

            if (!down) tileMap.SetCell(wallsLayer, position, 0, new Vector2I(1, 9));
            if (!down && !right) tileMap.SetCell(wallsLayer, position, 0, new Vector2I(2, 9));
            if (!down && !left) tileMap.SetCell(wallsLayer, position, 0, new Vector2I(0, 9));

            if (down && !downDown) tileMap.SetCell(wallsLayer, position, 0, new Vector2I(1, 8));
            if (down && !downRight) tileMap.SetCell(wallsLayer, position, 0, new Vector2I(2, 8));
            if (down && !downLeft) tileMap.SetCell(wallsLayer, position, 0, new Vector2I(0, 8));

            if (down && !downRight && right) tileMap.SetCell(wallsLayer, position, 0, new Vector2I(3, 9));
            if (down && !downLeft && left) tileMap.SetCell(wallsLayer, position, 0, new Vector2I(4, 9));
        }

        tileMap.ClearLayer(shadowLayer);

        foreach (Vector2I position in wallUsedCells) {
            bool right = WallTileAt(wallsLayer, position + Vector2I.Right, tileMap);
            bool left = WallTileAt(wallsLayer, position + Vector2I.Left, tileMap);
            bool down = WallTileAt(wallsLayer, position + Vector2I.Down, tileMap);

            bool downRight = WallTileAt(wallsLayer, position + Vector2I.Down + Vector2I.Right, tileMap);
            bool downLeft = WallTileAt(wallsLayer, position + Vector2I.Down + Vector2I.Left, tileMap);

            bool downDown = WallTileAt(wallsLayer, position + Vector2I.Down * 2, tileMap);

            if (!down) tileMap.SetCell(shadowLayer, position + Vector2I.Down, 0, new Vector2I(1, 11));
            if (!down && !right) tileMap.SetCell(shadowLayer, position + Vector2I.Down, 0, new Vector2I(2, 11));
            if (!down && !left) tileMap.SetCell(shadowLayer, position + Vector2I.Down, 0, new Vector2I(0, 11));

            if (down && !downDown) tileMap.SetCell(shadowLayer, position + Vector2I.Down, 0, new Vector2I(1, 10));
            if (down && !downRight) tileMap.SetCell(shadowLayer, position + Vector2I.Down, 0, new Vector2I(2, 10));
            if (down && !downLeft) tileMap.SetCell(shadowLayer, position + Vector2I.Down, 0, new Vector2I(0, 10));

            if (down && !downRight && right) {
                tileMap.SetCell(shadowLayer, position + Vector2I.Down, 0, new Vector2I(3, 10));
                tileMap.SetCell(shadowLayer, position, 0, new Vector2I(1, 10));
            }

            if (down && !downLeft && left) {
                tileMap.SetCell(shadowLayer, position + Vector2I.Down, 0, new Vector2I(4, 10));
                tileMap.SetCell(shadowLayer, position, 0, new Vector2I(1, 10));
            }
        }
    }

    private bool RoofTileAt(int layer, Vector2I position, TileMap tileMap) {
        return tileMap.GetCellTileData(layer, position) != null && tileMap.GetCellTileData(layer, position + Vector2I.Down) != null;
    }

    private bool WallTileAt(int layer, Vector2I position, TileMap tileMap) {
        return tileMap.GetCellTileData(layer, position) != null;
    }
}