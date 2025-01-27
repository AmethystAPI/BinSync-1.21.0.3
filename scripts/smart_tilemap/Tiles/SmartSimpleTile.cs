using System;
using Godot;

public partial class SmartSimpleTile : SmartTile {
    public Vector2 SimpleTile;

    protected override Vector2? GetTileLocation(Vector2I location, Func<Vector2I, bool> isTile) {
        return SimpleTile;
    }

    protected override Vector2I GetCenter() {
        return (Vector2I)SimpleTile;
    }
}