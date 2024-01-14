using System.Collections.Generic;
using Godot;

public partial class RoomPlacer : Resource {
    [Export] public PackedScene RoomScene;

    [Export] public bool LeftConnection;
    [Export] public bool RightConnection;
    [Export] public bool TopConnection;
    [Export] public bool BottomConnection;

    public List<Vector2> GetDirections() {
        List<Vector2> directions = new List<Vector2>();

        if (LeftConnection) directions.Add(Vector2.Left);
        if (RightConnection) directions.Add(Vector2.Right);
        if (TopConnection) directions.Add(Vector2.Up);
        if (BottomConnection) directions.Add(Vector2.Down);

        return directions;
    }
}