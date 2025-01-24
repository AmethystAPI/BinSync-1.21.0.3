using System;
using Godot;

[Tool]
public partial class SmartTileModifier : Resource {
    public virtual Vector2I Modify(Vector2I center, Vector2I location) {
        return location;
    }
}