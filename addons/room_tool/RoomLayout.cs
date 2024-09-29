using Godot;

[Tool]
public partial class RoomLayout : Resource {
    [Export] public Vector2[] Connections;
    [Export] public Vector2[] Walls;
    [Export] public Vector2 TopLeftBound;
    [Export] public Vector2 BottomRightBound;
}