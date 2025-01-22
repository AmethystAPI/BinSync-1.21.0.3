using System;
using Godot;

[Tool]
public partial class SmartShadowTile : SmartTile {
    [Export] public Vector2 ShadowCenter;

    private Vector2 ShadowLeft = new Vector2(-1, 0);
    private Vector2 ShadowRight = new Vector2(1, 0);
    private Vector2 ShadowInsideLeft = new Vector2(2, 0);
    private Vector2 ShadowInsideRight = new Vector2(3, 0);
    private Vector2 ShadowHidden = new Vector2(4, 0);

    protected override Vector2? GetTileLocation(Vector2I location, Func<Vector2I, bool> isTile) {
        bool upRight = isTile(location + Vector2I.Up + Vector2I.Right);
        bool upLeft = isTile(location + Vector2I.Up + Vector2I.Left);

        bool center = isTile(location);
        bool right = isTile(location + Vector2I.Right);
        bool left = isTile(location + Vector2I.Left);

        bool down = isTile(location + Vector2I.Down);
        bool downRight = isTile(location + Vector2I.Down + Vector2I.Right);
        bool downLeft = isTile(location + Vector2I.Down + Vector2I.Left);

        if (down && !downRight && right) return ShadowCenter + Vector2.Up;
        if (down && !downLeft && left) return ShadowCenter + Vector2.Up;

        if (center && !right && upRight) return ShadowCenter + ShadowInsideLeft;
        if (center && !left && upLeft) return ShadowCenter + ShadowInsideRight;

        if (center && !down && !right) return ShadowCenter + ShadowRight + Vector2.Up;
        if (center && !down && !left) return ShadowCenter + ShadowLeft + Vector2.Up;
        if (center && !down) return ShadowCenter + Vector2.Up;

        if (!center && !upRight) return ShadowCenter + ShadowRight;
        if (!center && !upLeft) return ShadowCenter + ShadowLeft;
        if (!center) return ShadowCenter;

        return null;
    }
}