using System.Collections.Generic;
using System.IO;
using Godot;

public partial class Biome : Resource {
    [Export] public PackedScene[] Rooms = new PackedScene[0];
    [Export] public PackedScene[] SpawnRooms = new PackedScene[0];
    [Export] public PackedScene[] FinalRooms = new PackedScene[0];
    [Export] public PackedScene[] FinalBranchRooms = new PackedScene[0];
    [Export] public Vector2[] BranchRanges = new Vector2[0];
    [Export] public int Level = 0;
    [Export] public Vector2I Size = new Vector2I(15, 20);
    [Export] public Vector2I BranchSize = new Vector2I(3, 5);
    [Export] public Color Color;

    public List<RoomLayout> RoomLayouts = new List<RoomLayout>();
    public List<RoomLayout> SpawnRoomLayouts = new List<RoomLayout>();
    public List<RoomLayout> FinalRoomLayouts = new List<RoomLayout>();
    public List<RoomLayout> FinalBranchRoomLayouts = new List<RoomLayout>();

    private bool _loaded = false;

    public void Load() {
        if (_loaded) throw new System.Exception("Biome already loaded!");

        foreach (PackedScene room in Rooms) {
            string layoutPath = GetRoomLayoutPath(room.ResourcePath);

            RoomLayouts.Add(ResourceLoader.Load<RoomLayout>(layoutPath));
        }

        foreach (PackedScene room in SpawnRooms) {
            string layoutPath = GetRoomLayoutPath(room.ResourcePath);

            SpawnRoomLayouts.Add(ResourceLoader.Load<RoomLayout>(layoutPath));
        }

        foreach (PackedScene room in FinalRooms) {
            string layoutPath = GetRoomLayoutPath(room.ResourcePath);

            FinalRoomLayouts.Add(ResourceLoader.Load<RoomLayout>(layoutPath));
        }

        foreach (PackedScene room in FinalBranchRooms) {
            string layoutPath = GetRoomLayoutPath(room.ResourcePath);

            FinalBranchRoomLayouts.Add(ResourceLoader.Load<RoomLayout>(layoutPath));
        }

        _loaded = true;
    }

    private string GetRoomLayoutPath(string path) {
        string relativePath = path.Substring("res://content/rooms/".Length);
        string fileName = Path.GetFileName(relativePath);
        string relativeFolders = relativePath.Substring(0, relativePath.Length - fileName.Length);
        string saveRelativePath = relativeFolders + "room_layout_" + Path.GetFileNameWithoutExtension(relativePath) + ".tres";

        return "res://generated/rooms/" + saveRelativePath;
    }
}