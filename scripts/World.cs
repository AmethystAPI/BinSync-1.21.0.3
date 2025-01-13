using System.Collections.Generic;
using System.Linq;
using Godot;
using Networking;

public partial class World : Node2D, NetworkPointUser {
    public static World Me;

    public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

    private TileMapLayer _wallsTileMapLayer;

    private List<WorldGenerator.RoomPlacement> _unloadedRoomPlacements = new List<WorldGenerator.RoomPlacement>();
    private Dictionary<WorldGenerator.RoomPlacement, float> _loadedRoomPlacements = new Dictionary<WorldGenerator.RoomPlacement, float>();

    public override void _Ready() {
        Me = this;

        _wallsTileMapLayer = GetNode<TileMapLayer>("Walls");
    }

    public override void _Process(double delta) {
        foreach (Player player in Player.Players) {
            Load(player.GlobalPosition);
        }

        Unload((float)delta);
    }

    public static void Start() {
        Me._loadedRoomPlacements = new Dictionary<WorldGenerator.RoomPlacement, float>();

        Stack<WorldGenerator.RoomPlacement> roomPlacements = WorldGenerator.Me.Generate(Game.Seed);

        foreach (WorldGenerator.RoomPlacement roomPlacement in roomPlacements) {
            Me._unloadedRoomPlacements.Add(roomPlacement);

            if (roomPlacement is WorldGenerator.BranchedRoomPlacement branchedRoomPlacement) {
                foreach (Stack<WorldGenerator.RoomPlacement> branches in branchedRoomPlacement.BranchRoomPlacements) {
                    foreach (WorldGenerator.RoomPlacement branchRoomPlacement in branches) {
                        Me._unloadedRoomPlacements.Add(branchRoomPlacement);
                    }
                }
            }
        }

        Me.Load(Vector2.Zero);
    }

    private void Load(Vector2 location) {
        List<WorldGenerator.RoomPlacement> loadedRoomPlacements = _loadedRoomPlacements.Keys.ToList();

        foreach (WorldGenerator.RoomPlacement roomPlacement in loadedRoomPlacements) {
            if (location.DistanceTo(roomPlacement.Location * 16) > 600) continue;

            _loadedRoomPlacements[roomPlacement] = 10;
        }

        for (int index = 0; index < _unloadedRoomPlacements.Count; index++) {
            WorldGenerator.RoomPlacement roomPlacement = _unloadedRoomPlacements[index];

            if (location.DistanceTo(roomPlacement.Location * 16) > 600) continue;

            _unloadedRoomPlacements.RemoveAt(index);
            index--;

            _loadedRoomPlacements.Add(roomPlacement, 10);

            LoadRoom(roomPlacement);
        }
    }

    private void Unload(float delta) {
        List<WorldGenerator.RoomPlacement> loadedRoomPlacements = _loadedRoomPlacements.Keys.ToList();

        foreach (WorldGenerator.RoomPlacement roomPlacement in loadedRoomPlacements) {
            _loadedRoomPlacements[roomPlacement] -= delta;

            if (_loadedRoomPlacements[roomPlacement] > 0) continue;

            _loadedRoomPlacements.Remove(roomPlacement);

            _unloadedRoomPlacements.Add(roomPlacement);

            UnloadRoom(roomPlacement);
        }
    }

    private void LoadRoom(WorldGenerator.RoomPlacement roomPlacement) {
        foreach (Vector2 tileLocation in roomPlacement.RoomLayout.Walls) {
            Vector2I realTileLocation = roomPlacement.Location + new Vector2I((int)tileLocation.X, (int)tileLocation.Y);

            _wallsTileMapLayer.SetCell(realTileLocation, 0, new Vector2I(3, 0));
        }
    }

    private void UnloadRoom(WorldGenerator.RoomPlacement roomPlacement) {
        foreach (Vector2 tileLocation in roomPlacement.RoomLayout.Walls) {
            Vector2I realTileLocation = roomPlacement.Location + new Vector2I((int)tileLocation.X, (int)tileLocation.Y);

            _wallsTileMapLayer.SetCell(realTileLocation, -1);
        }
    }

    private void PlaceRoom(WorldGenerator.RoomPlacement placement) {
        foreach (Vector2 tileLocation in placement.RoomLayout.Walls) {
            Vector2I realTileLocation = placement.Location + new Vector2I((int)tileLocation.X, (int)tileLocation.Y);

            _wallsTileMapLayer.SetCell(realTileLocation, 0, new Vector2I(3, 0));
        }

        if (placement is WorldGenerator.BranchedRoomPlacement branchPlacement) {
            foreach (Stack<WorldGenerator.RoomPlacement> branchStack in branchPlacement.BranchRoomPlacements) {
                foreach (WorldGenerator.RoomPlacement branchRoomPlacement in branchStack) {
                    foreach (Vector2 tileLocation in branchRoomPlacement.RoomLayout.Walls) {
                        Vector2I realTileLocation = branchRoomPlacement.Location + new Vector2I((int)tileLocation.X, (int)tileLocation.Y);

                        _wallsTileMapLayer.SetCell(realTileLocation, 0, new Vector2I(3, 0));
                    }
                }
            }
        }
    }
}
