using Godot;

public partial class Biome : Resource {
    [Export] public RoomPlacer[] RoomPlacers = new RoomPlacer[0];
    [Export] public UniqueEncounter[] UniqueEncounters = new UniqueEncounter[0];
    [Export] public int Level = 0;
}