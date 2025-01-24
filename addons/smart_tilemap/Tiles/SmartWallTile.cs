using System;
using Godot;

[Tool]
public partial class SmartWallTile : SmartTile {
    [Export] public Vector2 WallCenter;

    private Vector2 WallLeft = new Vector2(-1, 0);
    private Vector2 WallRight = new Vector2(1, 0);
    private Vector2 WallInsideLeft = new Vector2(2, 0);
    private Vector2 WallInsideRight = new Vector2(3, 0);
    private Vector2 WallHidden = new Vector2(4, 0);

    protected override Vector2? GetTileLocation(Vector2I location, Func<Vector2I, bool> isTile) {
        bool right = isTile(location + Vector2I.Right);
        bool left = isTile(location + Vector2I.Left);
        bool down = isTile(location + Vector2I.Down);

        bool downRight = isTile(location + Vector2I.Down + Vector2I.Right);
        bool downLeft = isTile(location + Vector2I.Down + Vector2I.Left);

        bool downDown = isTile(location + Vector2I.Down * 2);

        if (down && !downRight && right) return WallCenter + WallInsideLeft;
        if (down && !downLeft && left) return WallCenter + WallInsideRight;

        if (down && !downDown && !downRight) return WallCenter + WallRight + Vector2.Up;
        if (down && !downDown && !downLeft) return WallCenter + WallLeft + Vector2.Up;
        if (down && !downDown) return WallCenter + Vector2.Up;

        if (!down && !right) return WallCenter + WallRight;
        if (!down && !left) return WallCenter + WallLeft;
        if (!down) return WallCenter;

        return WallCenter + WallHidden;
    }

    protected override Vector2I GetCenter() {
        return (Vector2I)WallCenter;
    }
}