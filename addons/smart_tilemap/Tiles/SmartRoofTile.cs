using System;
using Godot;

[Tool]
public partial class SmartRoofTile : SmartTile {
    [Export] public Vector2 RoofCenter;

    private Vector2 RoofTopLeft = new Vector2(-1, -1);
    private Vector2 RoofTop = new Vector2(0, -1);
    private Vector2 RoofTopRight = new Vector2(1, -1);
    private Vector2 RoofLeft = new Vector2(-1, 0);
    private Vector2 RoofRight = new Vector2(1, 0);
    private Vector2 RoofBottomLeft = new Vector2(-1, 1);
    private Vector2 RoofBottom = new Vector2(0, 1);
    private Vector2 RoofBottomRight = new Vector2(1, 1);
    private Vector2 RoofInsideTopLeft = new Vector2(-1, 2);
    private Vector2 RoofInsideTopRight = new Vector2(0, 2);
    private Vector2 RoofInsideBottomLeft = new Vector2(-1, 3);
    private Vector2 RoofInsideBottomRight = new Vector2(0, 3);

    protected override Vector2? GetTileLocation(Vector2I location, Func<Vector2I, bool> isTile) {
        if (!isTile(location + Vector2I.Down)) return null;

        bool right = IsTileRoofOrBounds(location + Vector2I.Right, isTile);
        bool left = IsTileRoofOrBounds(location + Vector2I.Left, isTile);
        bool up = IsTileRoofOrBounds(location + Vector2I.Up, isTile);
        bool down = IsTileRoofOrBounds(location + Vector2I.Down, isTile);

        bool upRight = IsTileRoofOrBounds(location + Vector2I.Up + Vector2I.Right, isTile);
        bool upLeft = IsTileRoofOrBounds(location + Vector2I.Up + Vector2I.Left, isTile);
        bool downRight = IsTileRoofOrBounds(location + Vector2I.Down + Vector2I.Right, isTile);
        bool downLeft = IsTileRoofOrBounds(location + Vector2I.Down + Vector2I.Left, isTile);

        if (left && up && !upLeft) return RoofCenter + RoofInsideBottomRight;
        if (!left && !up && !upLeft) return RoofCenter + RoofTopLeft;

        if (right && up && !upRight) return RoofCenter + RoofInsideBottomLeft;
        if (!right && !up && !upRight) return RoofCenter + RoofTopRight;

        if (left && down && !downLeft) return RoofCenter + RoofInsideTopRight;
        if (!left && !down && !downLeft) return RoofCenter + RoofBottomLeft;

        if (right && down && !downRight) return RoofCenter + RoofInsideTopLeft;
        if (!right && !down && !downRight) return RoofCenter + RoofBottomRight;

        if (!right) return RoofCenter + RoofRight;
        if (!left) return RoofCenter + RoofLeft;
        if (!up) return RoofCenter + RoofTop;
        if (!down) return RoofCenter + RoofBottom;

        return RoofCenter;
    }

    private bool IsTileRoofOrBounds(Vector2I location, Func<Vector2I, bool> isWallTile) {
        return isWallTile(location) && isWallTile(location + Vector2I.Down);
    }

    protected override Vector2I GetCenter() {
        return (Vector2I)RoofCenter;
    }
}