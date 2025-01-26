using Godot;

public class Assets {
    public static void Load() {
        AssetManager.Load("res://content");
        AssetManager.Load("res://generated");

        CreateBiomes();
    }

    public static void CreateBiomes() {
        AssetManager.Register("biome_golden_grove", new Biome() {
            Rooms = new PackedScene[] {
                AssetManager.Get<PackedScene>("room_golden_grove_twist")
            },
            SpawnRooms = new PackedScene[] {
                 AssetManager.Get<PackedScene>("room_golden_grove_spawn")
            },
            FinalRooms = new PackedScene[] {
                AssetManager.Get<PackedScene>("room_golden_grove_final")
            },
            Color = new Color("#8a361e"),
        });
    }
}