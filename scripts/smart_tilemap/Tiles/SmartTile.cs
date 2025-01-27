using System;
using Godot;

public partial class SmartTile {
    public struct Tile {
        public int Source;
        public Vector2I Location;
    }

    public string Id;
    public int Source;
    public delegate Vector2I Modifier(Vector2I center, Vector2I location);
    public Modifier[] Modifiers;

    protected virtual Vector2? GetTileLocation(Vector2I location, Func<Vector2I, bool> isTile) {
        return null;
    }

    protected virtual Vector2I GetCenter() {
        return Vector2I.Zero;
    }

    protected Vector2I ApplyModifiers(Vector2I location) {
        Vector2I center = GetCenter();

        if (Modifiers != null) {
            foreach (Modifier smartTileModifier in Modifiers) {
                location = smartTileModifier(center, location);
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