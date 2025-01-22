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

    protected override Vector2? GetTileLocation(Vector2I location, Func<Vector2I, bool> isTile) {
        bool right = isTile(location + Vector2I.Right);
        bool left = isTile(location + Vector2I.Left);
        bool up = isTile(location + Vector2I.Up);
        bool down = isTile(location + Vector2I.Down);

        bool center = isTile(location);

        bool upRight = isTile(location + Vector2I.Up + Vector2I.Right);
        bool upLeft = isTile(location + Vector2I.Up + Vector2I.Left);
        bool downRight = isTile(location + Vector2I.Down + Vector2I.Right);
        bool downLeft = isTile(location + Vector2I.Down + Vector2I.Left);

        // if (left && up && !upLeft) return Center + InsideBottomRight;
        // if (!left && !up && !upLeft) return Center + TopLeft;

        // if (right && up && !upRight) return Center + InsideBottomLeft;
        // if (!right && !up && !upRight) return Center + TopRight;

        // if (left && down && !downLeft) return Center + InsideTopRight;
        // if (!left && !down && !downLeft) return Center + BottomLeft;

        // if (right && down && !downRight) return Center + InsideTopLeft;
        // if (!right && !down && !downRight) return Center + BottomRight;

        if (center && left) return Center + Top;
        if (!center && up && upLeft) return Center + Bottom;
        if (!right) return Center + Right;
        if (!left) return Center + Left;

        return Center;
    }
}