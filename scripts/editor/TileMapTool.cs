using System.Collections.Generic;
using System.Linq;
using Godot;

[Tool]
public partial class TileMapTool : Node {
#if TOOLS
    [Export] public SmartTileset SmartTileset;

    private bool _justPressedGenerate = false;

    private float _timer = 0.5f;

    public override void _Process(double delta) {
        // if (Input.IsKeyPressed(Key.Backslash) && _justPressedGenerate) return;

        // if (!Input.IsKeyPressed(Key.Backslash)) {
        //     _justPressedGenerate = false;

        //     return;
        // }

        // _justPressedGenerate = true;

        _timer -= (float)delta;

        if (_timer <= 0) {
            Generate(GetParent<TileMap>());

            _timer = 0.5f;
        }
    }

    private void Generate(TileMap tileMap) {
        if (SmartTileset == null) return;

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

        int leftBound = wallUsedCells.Min(position => position.X);
        int rightBound = wallUsedCells.Max(position => position.X);
        int upBound = wallUsedCells.Min(position => position.Y);
        int downBound = wallUsedCells.Max(position => position.Y);

        Rect2I rect = new Rect2I(leftBound, upBound, rightBound - leftBound + 1, downBound - upBound + 1);

        foreach (Vector2I position in wallUsedCells) {
            if (tileMap.GetCellTileData(wallsLayer, position + Vector2I.Down) == null && position.Y != downBound) continue;

            tileMap.SetCell(roofsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.RoofsMiddle));

            bool right = RoofTileAt(wallsLayer, position + Vector2I.Right, tileMap, rect);
            bool left = RoofTileAt(wallsLayer, position + Vector2I.Left, tileMap, rect);
            bool up = RoofTileAt(wallsLayer, position + Vector2I.Up, tileMap, rect);
            bool down = RoofTileAt(wallsLayer, position + Vector2I.Down, tileMap, rect);

            bool upRight = RoofTileAt(wallsLayer, position + Vector2I.Up + Vector2I.Right, tileMap, rect);
            bool upLeft = RoofTileAt(wallsLayer, position + Vector2I.Up + Vector2I.Left, tileMap, rect);
            bool downRight = RoofTileAt(wallsLayer, position + Vector2I.Down + Vector2I.Right, tileMap, rect);
            bool downLeft = RoofTileAt(wallsLayer, position + Vector2I.Down + Vector2I.Left, tileMap, rect);

            if (!right) tileMap.SetCell(roofsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.RoofsRight));
            if (!left) tileMap.SetCell(roofsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.RoofsLeft));
            if (!up) tileMap.SetCell(roofsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.RoofsTop));
            if (!down) tileMap.SetCell(roofsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.RoofsBottom));

            if (!right && !down && !downRight) tileMap.SetCell(roofsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.RoofsBottomRight));
            if (right && down && !downRight) tileMap.SetCell(roofsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.RoofsInsideTopLeft));

            if (!left && !down && !downLeft) tileMap.SetCell(roofsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.RoofsBottomLeft));
            if (left && down && !downLeft) tileMap.SetCell(roofsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.RoofsInsideTopRight));

            if (!right && !up && !upRight) tileMap.SetCell(roofsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.RoofsTopRight));
            if (right && up && !upRight) tileMap.SetCell(roofsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.RoofsInsideBottomLeft));

            if (!left && !up && !upLeft) tileMap.SetCell(roofsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.RoofsTopLeft));
            if (left && up && !upLeft) tileMap.SetCell(roofsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.RoofsInsideBottomRight));
        }

        foreach (Vector2I position in wallUsedCells) {
            tileMap.SetCell(wallsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.WallHidden);

            bool right = WallTileAt(wallsLayer, position + Vector2I.Right, tileMap, rect);
            bool left = WallTileAt(wallsLayer, position + Vector2I.Left, tileMap, rect);
            bool down = WallTileAt(wallsLayer, position + Vector2I.Down, tileMap, rect);

            bool downRight = WallTileAt(wallsLayer, position + Vector2I.Down + Vector2I.Right, tileMap, rect);
            bool downLeft = WallTileAt(wallsLayer, position + Vector2I.Down + Vector2I.Left, tileMap, rect);

            bool downDown = WallTileAt(wallsLayer, position + Vector2I.Down * 2, tileMap, rect);

            if (!down) tileMap.SetCell(wallsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.WallsMiddle));
            if (!down && !right) tileMap.SetCell(wallsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.WallsRight));
            if (!down && !left) tileMap.SetCell(wallsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.WallsLeft));

            if (down && !downDown) tileMap.SetCell(wallsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.WallsMiddle) + Vector2I.Up);
            if (down && !downDown && !downRight) tileMap.SetCell(wallsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.WallsRight) + Vector2I.Up);
            if (down && !downDown && !downLeft) tileMap.SetCell(wallsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.WallsLeft) + Vector2I.Up);

            if (down && !downRight && right) tileMap.SetCell(wallsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.WallsInsideLeft));
            if (down && !downLeft && left) tileMap.SetCell(wallsLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.GetRandom(SmartTileset.WallsInsideRight));
        }

        tileMap.ClearLayer(shadowLayer);

        foreach (Vector2I position in wallUsedCells) {
            bool right = WallTileAt(wallsLayer, position + Vector2I.Right, tileMap, rect);
            bool left = WallTileAt(wallsLayer, position + Vector2I.Left, tileMap, rect);
            bool down = WallTileAt(wallsLayer, position + Vector2I.Down, tileMap, rect);

            bool downRight = WallTileAt(wallsLayer, position + Vector2I.Down + Vector2I.Right, tileMap, rect);
            bool downLeft = WallTileAt(wallsLayer, position + Vector2I.Down + Vector2I.Left, tileMap, rect);

            bool downDown = WallTileAt(wallsLayer, position + Vector2I.Down * 2, tileMap, rect);

            if (!down) tileMap.SetCell(shadowLayer, position + Vector2I.Down, SmartTileset.SourceId, (Vector2I)SmartTileset.ShadowMiddle);
            if (!down && !right) tileMap.SetCell(shadowLayer, position + Vector2I.Down, SmartTileset.SourceId, (Vector2I)SmartTileset.ShadowRight);
            if (!down && !left) tileMap.SetCell(shadowLayer, position + Vector2I.Down, SmartTileset.SourceId, (Vector2I)SmartTileset.ShadowLeft);

            if (down && !downDown) tileMap.SetCell(shadowLayer, position + Vector2I.Down, SmartTileset.SourceId, (Vector2I)SmartTileset.ShadowMiddle + Vector2I.Up);
            if (down && !downDown && !downRight) tileMap.SetCell(shadowLayer, position + Vector2I.Down, SmartTileset.SourceId, (Vector2I)SmartTileset.ShadowRight + Vector2I.Up);
            if (down && !downDown && !downLeft) tileMap.SetCell(shadowLayer, position + Vector2I.Down, SmartTileset.SourceId, (Vector2I)SmartTileset.ShadowLeft + Vector2I.Up);

            if (down && !downRight && right) {
                tileMap.SetCell(shadowLayer, position + Vector2I.Down, SmartTileset.SourceId, (Vector2I)SmartTileset.ShadowInsideLeft);
                tileMap.SetCell(shadowLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.ShadowMiddle);
            }

            if (down && !downLeft && left) {
                tileMap.SetCell(shadowLayer, position + Vector2I.Down, SmartTileset.SourceId, (Vector2I)SmartTileset.ShadowInsideRight);
                tileMap.SetCell(shadowLayer, position, SmartTileset.SourceId, (Vector2I)SmartTileset.ShadowMiddle);
            }
        }
    }

    private bool RoofTileAt(int layer, Vector2I position, TileMap tileMap, Rect2I rect) {
        return (tileMap.GetCellTileData(layer, position) != null || !rect.HasPoint(position)) && (tileMap.GetCellTileData(layer, position + Vector2I.Down) != null || !rect.HasPoint(position + Vector2I.Down));
    }

    private bool WallTileAt(int layer, Vector2I position, TileMap tileMap, Rect2I rect) {
        return tileMap.GetCellTileData(layer, position) != null || !rect.HasPoint(position);
    }
#endif
}