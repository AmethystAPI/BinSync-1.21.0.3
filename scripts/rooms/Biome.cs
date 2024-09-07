using Godot;

public partial class Biome : Resource {
    [Export] public RoomPlacer[] RoomPlacers = new RoomPlacer[0];
    [Export] public UniqueEncounter[] UniqueEncounters = new UniqueEncounter[0];
    [Export] public int Level = 0;
    [Export] public Vector2I Size = new Vector2I(15, 20);
    [Export] public Color Color;
}