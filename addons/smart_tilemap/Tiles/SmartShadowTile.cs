using System;
using Godot;

[Tool]
public partial class SmartShadowTile : SmartTile {
    [Export] public Vector2 ShadowLeft;
    [Export] public Vector2 ShadowMiddle;
    [Export] public Vector2 ShadowRight;
    [Export] public Vector2 ShadowInsideLeft;
    [Export] public Vector2 ShadowInsideRight;

    protected override Vector2? GetTileLocation(Vector2I location, Func<Vector2I, bool> isTile) {
        // bool right = isWallTile(location + Vector2I.Right);
        // bool left = isWallTile(location + Vector2I.Left);
        // bool down = isWallTile(location + Vector2I.Down);

        // bool downRight = isWallTile(location + Vector2I.Down + Vector2I.Right);
        // bool downLeft = isWallTile(location + Vector2I.Down + Vector2I.Left);

        // bool downDown = isWallTile(location + Vector2I.Down * 2);

        // if (down && !downRight && right) return (ShadowInsideLeft, ShadowMiddle + Vector2.Up);
        // if (down && !downLeft && left) return (ShadowInsideRight, ShadowMiddle + Vector2.Up);

        // if (down && !downDown && !downRight) return (ShadowRight + Vector2.Up, null);
        // if (down && !downDown && !downLeft) return (ShadowLeft + Vector2.Up, null);
        // if (down && !downDown) return (ShadowMiddle + Vector2.Up, null);

        // if (!down && !right) return (ShadowRight, null);
        // if (!down && !left) return (ShadowLeft, null);
        // if (!down) return (ShadowMiddle, null);

        // return (null, null);

        return null;
    }
}