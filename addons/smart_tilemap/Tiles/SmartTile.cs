using System;
using Godot;

[Tool]
public partial class SmartTile : Resource {
    public struct Tile {
        public int Source;
        public Vector2I Location;
    }

    [Export] public string Id;
    [Export] public int Source;

    protected virtual Vector2? GetTileLocation(Vector2I location, Func<Vector2I, bool> isTile) {
        return null;
    }

    public virtual Tile? GetTile(Vector2I location, Func<Vector2I, bool> isTile) {
        Vector2? possibleTileLocation = GetTileLocation(location, isTile);

        if (!(possibleTileLocation is Vector2 tileLocation)) return null;

        return new Tile {
            Location = (Vector2I)tileLocation,
            Source = Source,
        };
    }
}