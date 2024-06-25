using Godot;

public partial class UniqueEncounter : Resource {
    [Export] public RoomPlacer RoomPlacer;
    [Export] public int Limit = 1;
    [Export] public Vector2I Interval = new Vector2I(3, 6);
    [Export] public int Priority = 1;
}