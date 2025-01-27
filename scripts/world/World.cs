using System.Collections.Generic;
using System.Linq;
using Godot;
using Networking;
using Riptide;

public partial class World : Node2D, NetworkPointUser {
    public static World Me;

    public TileMapLayer WallsTileMapLayer;
    public TileMapLayer RoofsTileMapLayer;
    public TileMapLayer ShadowsTileMapLayer;
    public TileMapLayer FloorsTileMapLayer;
    public TileMapLayer GrassTileMapLayer;

    public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

    private Biome[] _biomes;
    private Biome _activeBiome;
    private List<LoadableRoom> _unloadedRooms = new List<LoadableRoom>();
    private Dictionary<LoadableRoom, float> _loadedRooms = new Dictionary<LoadableRoom, float>();

    public override void _Ready() {
        Me = this;

        NetworkPoint.Setup(this);

        NetworkPoint.Register(nameof(SpawnEnemyRpc), SpawnEnemyRpc);

        WallsTileMapLayer = GetNode<TileMapLayer>("Walls");
        RoofsTileMapLayer = GetNode<TileMapLayer>("Roofs");
        ShadowsTileMapLayer = GetNode<TileMapLayer>("Shadows");
        FloorsTileMapLayer = GetNode<TileMapLayer>("Floors");
        GrassTileMapLayer = GetNode<TileMapLayer>("Grass");

        _biomes = new Biome[] { AssetManager.Get<Biome>("biome_golden_grove") };

        foreach (Biome biome in _biomes) {
            biome.Load();
        }
    }

    public override void _Process(double delta) {
        foreach (Player player in Player.Players) {
            Load(player.GlobalPosition);
        }

        Unload((float)delta);
    }

    public static void Start() {
        Me._loadedRooms = new Dictionary<LoadableRoom, float>();

        Me._activeBiome = Me._biomes[0];

        Stack<WorldGenerator.RoomPlacement> roomPlacements = WorldGenerator.Me.Generate(Game.Seed, Me._activeBiome);

        foreach (WorldGenerator.RoomPlacement roomPlacement in roomPlacements) {
            Me._unloadedRooms.Add(new LoadableRoom(roomPlacement, Me, Me._activeBiome));

            if (roomPlacement is WorldGenerator.BranchedRoomPlacement branchedRoomPlacement) {
                foreach (Stack<WorldGenerator.RoomPlacement> branches in branchedRoomPlacement.BranchRoomPlacements) {
                    foreach (WorldGenerator.RoomPlacement branchRoomPlacement in branches) {
                        Me._unloadedRooms.Add(new LoadableRoom(branchRoomPlacement, Me, Me._activeBiome));
                    }
                }
            }
        }

        Me.Load(Vector2.Zero);
    }

    private void Load(Vector2 location) {
        List<LoadableRoom> loadedRooms = _loadedRooms.Keys.ToList();

        foreach (LoadableRoom room in loadedRooms) {
            if (location.DistanceTo(room.RoomPlacement.Location * 16) > 600) continue;

            _loadedRooms[room] = 10;
        }

        for (int index = 0; index < _unloadedRooms.Count; index++) {
            LoadableRoom loadableRoom = _unloadedRooms[index];

            if (location.DistanceTo(loadableRoom.RoomPlacement.Location * 16) > 600) continue;

            _unloadedRooms.RemoveAt(index);
            index--;

            _loadedRooms.Add(loadableRoom, 10);

            loadableRoom.Load();
        }
    }

    private void Unload(float delta) {
        List<LoadableRoom> loadedRooms = _loadedRooms.Keys.ToList();

        foreach (LoadableRoom room in loadedRooms) {
            _loadedRooms[room] -= delta;

            if (_loadedRooms[room] > 0) {
                room.Update(delta);

                continue;
            }

            _loadedRooms.Remove(room);
            _unloadedRooms.Add(room);

            room.Unload();
        }
    }

    private LoadableRoom GetRoom(string Id) {
        return _loadedRooms.Keys.ToList().Find(room => room.Id == Id);
    }

    // private void PlaceRoom(WorldGenerator.RoomPlacement placement) {
    //     foreach (Vector2 tileLocation in placement.RoomLayout.Walls) {
    //         Vector2I realTileLocation = placement.Location + new Vector2I((int)tileLocation.X, (int)tileLocation.Y);

    //         WallsTileMapLayer.SetCell(realTileLocation, 0, new Vector2I(3, 0));
    //     }

    //     if (placement is WorldGenerator.BranchedRoomPlacement branchPlacement) {
    //         foreach (Stack<WorldGenerator.RoomPlacement> branchStack in branchPlacement.BranchRoomPlacements) {
    //             foreach (WorldGenerator.RoomPlacement branchRoomPlacement in branchStack) {
    //                 foreach (Vector2 tileLocation in branchRoomPlacement.RoomLayout.Walls) {
    //                     Vector2I realTileLocation = branchRoomPlacement.Location + new Vector2I((int)tileLocation.X, (int)tileLocation.Y);

    //                     WallsTileMapLayer.SetCell(realTileLocation, 0, new Vector2I(3, 0));
    //                 }
    //             }
    //         }
    //     }
    // }

    public void SpawnEnemyRpc(Message message) {
        Vector2 position = new Vector2(message.GetFloat(), message.GetFloat());
        string enemyScenePath = message.GetString();
        string roomId = message.GetString();

        Enemy enemy = NetworkManager.SpawnNetworkSafe<Enemy>(ResourceLoader.Load<PackedScene>(enemyScenePath), "Enemy");

        PackedScene spawnDustScene = ResourceLoader.Load<PackedScene>("res://scenes/particles/spawn_dust.tscn");
        Node2D spawnDust = spawnDustScene.Instantiate<Node2D>();

        AddChild(spawnDust);

        spawnDust.GlobalPosition = position;

        LoadableRoom room = GetRoom(roomId);
        room.AddEnemy(enemy);

        Delay.Execute(1, () => {
            if (!IsInstanceValid(this)) return;

            AddChild(enemy);

            enemy.GlobalPosition = position;

            enemy.Activate();
        });
    }
}
