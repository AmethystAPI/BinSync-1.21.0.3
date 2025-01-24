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
    [Export] public SmartTileModifier[] Modifiers;

    protected virtual Vector2? GetTileLocation(Vector2I location, Func<Vector2I, bool> isTile) {
        return null;
    }

    protected virtual Vector2I GetCenter() {
        return Vector2I.Zero;
    }

    protected Vector2I ApplyModifiers(Vector2I location) {
        Vector2I center = GetCenter();

        if (Modifiers != null) {
            foreach (SmartTileModifier smartTileModifier in Modifiers) {
                location = smartTileModifier.Modify(center, location);
            }
        }

        return location;
    }

    public Tile? GetTile(Vector2I location, Func<Vector2I, bool> isTile) {
        Vector2? possibleTileLocation = GetTileLocation(location, isTile);

        if (!(possibleTileLocation is Vector2 tileLocation)) return null;

        return new Tile {
            Location = ApplyModifiers((Vector2I)tileLocation),
            Source = Source,
        };
    }
}