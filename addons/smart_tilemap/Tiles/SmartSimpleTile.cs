using System;
using Godot;

[Tool]
public partial class SmartSimpleTile : SmartTile {
    [Export] public Vector2 SimpleTile;

    protected override Vector2? GetTileLocation(Vector2I location, Func<Vector2I, bool> isTile) {
        return SimpleTile;
    }
}