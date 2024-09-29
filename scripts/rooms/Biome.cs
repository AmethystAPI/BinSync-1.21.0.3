using System.IO;
using Godot;

public partial class Biome : Resource {
    [Export] public PackedScene[] Rooms = new PackedScene[0];
    [Export] public PackedScene[] SpawnRooms = new PackedScene[0];
    [Export] public RoomPlacer[] RoomPlacers = new RoomPlacer[0];
    [Export] public UniqueEncounter[] UniqueEncounters = new UniqueEncounter[0];
    [Export] public int Level = 0;
    [Export] public Vector2I Size = new Vector2I(15, 20);
    [Export] public Color Color;

    public RoomLayout GetRoomLayout(int index) {
        PackedScene room = Rooms[index];

        string layoutPath = GetRoomLayoutPath(room.ResourcePath);

        return ResourceLoader.Load<RoomLayout>(layoutPath);
    }

    private string GetRoomLayoutPath(string path) {
        string relativePath = path.Substring("res://content/rooms/".Length);
        string fileName = Path.GetFileName(relativePath);
        string relativeFolders = relativePath.Substring(0, relativePath.Length - fileName.Length);
        string saveRelativePath = relativeFolders + "room_layout_" + Path.GetFileNameWithoutExtension(relativePath) + ".tres";

        return "res://generated/rooms/" + saveRelativePath;
    }
}