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
            Tileset = AssetManager.Get<SmartTileset>("smart_tileset_golden_grove"),
            EnemyPool = new EnemyPool {
                Entries = new EnemyPool.Entry[] {
                    new EnemyPool.Entry(AssetManager.Get<PackedScene>("enemy_slime"), 1f),
                    new EnemyPool.Entry(AssetManager.Get<PackedScene>("enemy_stone_golem"), 1f),
                    new EnemyPool.Entry(AssetManager.Get<PackedScene>("enemy_crow"), 0.3f),
                    new EnemyPool.Entry(AssetManager.Get<PackedScene>("enemy_log_spirit"), 2f),
                }
            }
        });
    }
}