using System;
using Godot;

[Tool]
public partial class SmartHalfShiftTile : SmartTile {
    [Export] public Vector2 Center;

    private Vector2 TopLeft = new Vector2(-1, -1);
    private Vector2 Top = new Vector2(0, -1);
    private Vector2 TopRight = new Vector2(1, -1);
    private Vector2 Left = new Vector2(-1, 0);
    private Vector2 Right = new Vector2(1, 0);
    private Vector2 BottomLeft = new Vector2(-1, 1);
    private Vector2 Bottom = new Vector2(0, 1);
    private Vector2 BottomRight = new Vector2(1, 1);
    private Vector2 InsideTopLeft = new Vector2(2, -1);
    private Vector2 InsideTopRight = new Vector2(3, -1);
    private Vector2 InsideBottomLeft = new Vector2(2, 0);
    private Vector2 InsideBottomRight = new Vector2(3, 0);
    private Vector2 DiagonalTopLeftBottomRight = new Vector2(2, 1);
    private Vector2 DiagonalTopRightBottomLeft = new Vector2(3, 1);

    protected override Vector2? GetTileLocation(Vector2I location, Func<Vector2I, bool> isTile) {
        bool center = isTile(location);
        bool right = isTile(location + Vector2I.Right);
        bool down = isTile(location + Vector2I.Down);
        bool downRight = isTile(location + Vector2I.Down + Vector2I.Right);

        if (center && right && down && downRight) return Center;

        if (!center && !downRight && down && right) return Center + DiagonalTopRightBottomLeft;
        if (center && downRight && !down && !right) return Center + DiagonalTopLeftBottomRight;

        if (center && right && !downRight && down) return Center + InsideTopLeft;
        if (center && right && downRight && !down) return Center + InsideTopRight;
        if (!center && right && downRight && down) return Center + InsideBottomRight;
        if (center && !right && downRight && down) return Center + InsideBottomLeft;

        if (down && downRight) return Center + Top;
        if (center && right) return Center + Bottom;
        if (center && down) return Center + Right;
        if (right && downRight) return Center + Left;

        if (center) return Center + BottomRight;
        if (right) return Center + BottomLeft;
        if (downRight) return Center + TopLeft;
        if (down) return Center + TopRight;

        return null;
    }
}