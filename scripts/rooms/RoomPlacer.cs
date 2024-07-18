using System.Collections.Generic;
using Godot;

public partial class RoomPlacer : Resource {
    [Export] public PackedScene RoomScene;

    [Export] public bool LeftConnection;
    [Export] public bool RightConnection;
    [Export] public bool TopConnection;
    [Export] public bool BottomConnection;
    [Export] public Vector2 ExitDirection = Vector2.Up;

    public List<Vector2> GetDirections() {
        List<Vector2> directions = new List<Vector2>();

        if (LeftConnection) directions.Add(Vector2.Left);
        if (RightConnection) directions.Add(Vector2.Right);
        if (TopConnection) directions.Add(Vector2.Up);
        if (BottomConnection) directions.Add(Vector2.Down);

        return directions;
    }

    public bool CanConnectTo(Vector2 direction) {
        if (LeftConnection && direction == Vector2.Right) return true;
        if (RightConnection && direction == Vector2.Left) return true;
        if (TopConnection && direction == Vector2.Down) return true;
        if (BottomConnection && direction == Vector2.Up) return true;

        return false;
    }
}