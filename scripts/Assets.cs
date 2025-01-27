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
                AssetManager.GetScene("room_golden_grove_twist")
            },
            SpawnRooms = new PackedScene[] {
                 AssetManager.GetScene("room_golden_grove_spawn")
            },
            FinalRooms = new PackedScene[] {
                AssetManager.GetScene("room_golden_grove_final")
            },
            Color = new Color("#8a361e"),
            Tileset = new SmartTileset {
                TileSet = AssetManager.Get<TileSet>("tileset_golden_grove"),
                Tiles = new SmartTile[] {
                    new SmartWallTile() {
                        Id = "walls",
                        WallCenter = new Vector2(1, 6)
                    },
                    new SmartRoofTile() {
                        Id = "roofs",
                        RoofCenter = new Vector2(1, 1)
                    },
                    new SmartSimpleTile() {
                        Id = "floors",
                        SimpleTile = new Vector2(4, 0)
                    },
                    new SmartShadowTile() {
                        Id = "shadows",
                        ShadowCenter = new Vector2(1, 8)
                    },
                    new SmartHalfShiftTile() {
                        Id = "grass",
                        Center = new Vector2(1, 10),
                        Modifiers = new SmartTile.Modifier[] {
                            CreateRandomVariantModifier(
                                new Vector2(0, 0),
                                new Vector2[] {
                                    new Vector2(-1, 2),
                                    new Vector2(0, 2),
                                    new Vector2(1, 2),
                                }
                            )
                        }
                    }
                }
            },
            EnemyPool = new EnemyPool {
                Entries = new EnemyPool.Entry[] {
                    new EnemyPool.Entry(AssetManager.GetScene("enemy_slime"), 1f),
                    new EnemyPool.Entry(AssetManager.GetScene("enemy_stone_golem"), 1f),
                    new EnemyPool.Entry(AssetManager.GetScene("enemy_crow"), 0.3f),
                    new EnemyPool.Entry(AssetManager.GetScene("enemy_log_spirit"), 2f),
                }
            }
        });
    }

    private static SmartTile.Modifier CreateRandomVariantModifier(Vector2 target, Vector2[] variants) {
        RandomNumberGenerator random = new RandomNumberGenerator();
        random.Seed = Game.Seed;

        uint seedCache = Game.Seed;

        return (Vector2I center, Vector2I location) => {
            if (seedCache != Game.Seed) {
                random.Seed = Game.Seed;
                seedCache = Game.Seed;
            }

            if (location == center + target) {
                int index = random.RandiRange(0, variants.Length);

                if (index == 0) {
                    return location;
                } else {
                    return center + (Vector2I)variants[index - 1];
                }
            }

            return location;
        };
    }
}