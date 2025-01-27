using System.Collections.Generic;
using Godot;

public class Assets {
    public static void Load() {
        AssetManager.Load("res://content");
        AssetManager.Load("res://generated");

        CreateBiomes();
    }

    public static void CreateBiomes() {
        AssetManager.Register("biome.golden_grove", new Biome() {
            Rooms = new PackedScene[] {
                AssetManager.GetScene("room.golden_grove.twist"),
                AssetManager.GetScene("room.golden_grove.turn_up_right"),
                AssetManager.GetScene("room.golden_grove.turn_up_left"),
                AssetManager.GetScene("room.golden_grove.turn_down_right"),
                AssetManager.GetScene("room.golden_grove.turn_down_left"),
                AssetManager.GetScene("room.golden_grove.plain"),
                AssetManager.GetScene("room.golden_grove.horizontal"),
                AssetManager.GetScene("room.golden_grove.branch"),
                AssetManager.GetScene("room.golden_grove.branch_left"),
            },
            SpawnRooms = new PackedScene[] {
                 AssetManager.GetScene("room.golden_grove.spawn")
            },
            FinalRooms = new PackedScene[] {
                AssetManager.GetScene("room.golden_grove.final")
            },
            FinalBranchRooms = new PackedScene[] {
                AssetManager.GetScene("room.golden_grove.branch_end_left"),
                AssetManager.GetScene("room.golden_grove.branch_end_right"),
                AssetManager.GetScene("room.golden_grove.branch_end_up"),
            },
            BranchRanges = new Vector2[] {
                new Vector2(1, 3),
                new Vector2(4, 8),
                new Vector2(9, 13),
            },
            Color = new Color("#8a361e"),
            Tileset = new SmartTileset {
                TileSet = AssetManager.Get<TileSet>("tileset.golden_grove"),
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
                    new EnemyPool.Entry(AssetManager.GetScene("enemy.slime"), 1f),
                    new EnemyPool.Entry(AssetManager.GetScene("enemy.stone_golem"), 1f),
                    new EnemyPool.Entry(AssetManager.GetScene("enemy.crow"), 0.3f),
                    new EnemyPool.Entry(AssetManager.GetScene("enemy.log_spirit"), 2f),
                }
            },
            Decorations = new Decoration[] {
                new Decoration {
                    Generate = (roomPlacement, openDecorationLocations) => {
                        RandomNumberGenerator random = new RandomNumberGenerator();

                        PackedScene treeScene = AssetManager.GetScene("decoration.golden_grove.tree");

                        int amount = (int)(openDecorationLocations.Count * 0.03f);

                        List<WorldGenerator.DecorationPlacement> placements = new List<WorldGenerator.DecorationPlacement>();

                        for(int index = 0; index < amount; index++) {
                            int randomLocationIndex = random.RandiRange(0, openDecorationLocations.Count - 1);

                            Vector2I location = openDecorationLocations[randomLocationIndex];

                            openDecorationLocations.Remove(location);

                            placements.Add(new WorldGenerator.DecorationPlacement {
                                Location = location,
                                Scene = treeScene
                            });
                        }

                        return placements;
                    }
                }
            },
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