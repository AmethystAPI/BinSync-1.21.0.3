using Godot;
using Networking;
using Riptide;
using System.Collections.Generic;
using System.Data;
using System.Linq;

public partial class WorldGenerator : Node2D, NetworkPointUser {
    private class RoomPlacement {
        public RoomLayout RoomLayout;
        public Vector2I Location;

        public virtual bool Intersects(RoomPlacement otherRoom) {
            if (otherRoom.GetTopLeftBound().X >= GetTopLeftBound().X && otherRoom.GetTopLeftBound().X < GetBottomRightBound().X && otherRoom.GetTopLeftBound().Y >= GetTopLeftBound().Y && otherRoom.GetTopLeftBound().Y < GetBottomRightBound().Y) return true;

            if (otherRoom.GetBottomRightBound().X > GetTopLeftBound().X && otherRoom.GetBottomRightBound().X <= GetBottomRightBound().X && otherRoom.GetTopLeftBound().Y >= GetTopLeftBound().Y && otherRoom.GetTopLeftBound().Y < GetBottomRightBound().Y) return true;

            if (otherRoom.GetTopLeftBound().X >= GetTopLeftBound().X && otherRoom.GetTopLeftBound().X < GetBottomRightBound().X && otherRoom.GetBottomRightBound().Y > GetTopLeftBound().Y && otherRoom.GetBottomRightBound().Y <= GetBottomRightBound().Y) return true;

            if (otherRoom.GetBottomRightBound().X > GetTopLeftBound().X && otherRoom.GetBottomRightBound().X <= GetBottomRightBound().X && otherRoom.GetBottomRightBound().Y > GetTopLeftBound().Y && otherRoom.GetBottomRightBound().Y <= GetBottomRightBound().Y) return true;

            return false;
        }

        protected Vector2 GetTopLeftBound() {
            return Location + RoomLayout.TopLeftBound;
        }

        protected Vector2 GetBottomRightBound() {
            return Location + RoomLayout.BottomRightBound;
        }
    }

    private class BranchedRoomPlacement : RoomPlacement {
        public List<Stack<RoomPlacement>> BranchRoomPlacements;

        public override bool Intersects(RoomPlacement otherRoom) {
            if (base.Intersects(otherRoom)) return true;

            foreach (Stack<RoomPlacement> branchStack in BranchRoomPlacements) {
                foreach (RoomPlacement roomPlacement in branchStack) {
                    if (roomPlacement.Intersects(otherRoom)) return true;
                }
            }

            return false;
        }
    }

    private static WorldGenerator s_Me;

    [Export] public Biome[] Biomes;

    public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

    private TileMapLayer _wallsTileMapLayer;

    private RandomNumberGenerator _random;

    public override void _Ready() {
        s_Me = this;

        NetworkPoint.Setup(this);

        _wallsTileMapLayer = GetNode<TileMapLayer>("Walls");
    }

    public void Start() {
        _random = new RandomNumberGenerator();

        foreach (Biome biome in Biomes) {
            biome.Load();
        }

        RoomLayout.Connection lastConnection;

        RoomLayout spawnRoomLayout = Biomes[0].SpawnRoomLayouts[0];

        Vector2 spawnRoomPlaceLocation = Vector2.Zero;
        lastConnection = spawnRoomLayout.GetConnections()[0];
        lastConnection.Location += spawnRoomPlaceLocation;

        RoomPlacement spawnRoomPlacement = new RoomPlacement {
            RoomLayout = spawnRoomLayout,
            Location = new Vector2I((int)spawnRoomPlaceLocation.X, (int)spawnRoomPlaceLocation.Y)
        };

        Stack<RoomPlacement> placedRooms = new Stack<RoomPlacement>();
        placedRooms.Push(spawnRoomPlacement);

        int size = _random.RandiRange(Biomes[0].Size.X, Biomes[0].Size.Y);

        bool result = TryPlaceRooms(Biomes[0], placedRooms, lastConnection, size - 1, size, 0);

        GD.Print(result);

        foreach (RoomPlacement placement in placedRooms) {
            PlaceRoom(placement);
        }
    }

    private bool TryPlaceRooms(Biome biome, Stack<RoomPlacement> placedRooms, RoomLayout.Connection lastConnection, int roomsToPlace, int size, int branches) {
        List<RoomLayout> roomLayouts = new List<RoomLayout>(biome.RoomLayouts).OrderBy(item => _random.Randf()).ToList();

        int roomIndex = size - roomsToPlace;

        bool mustBranch = branches < biome.BranchRanges.Length && biome.BranchRanges.Any(range => range.Y == roomIndex);

        if (mustBranch) {
            roomLayouts = roomLayouts.Where(layout => layout.GetConnectionCount() > 2).ToList();

            GD.Print("Must place branch " + roomIndex);
        } else {
            bool couldBranch = branches < biome.BranchRanges.Length && biome.BranchRanges[branches..biome.BranchRanges.Length].Any(range => roomIndex >= range.X);

            if (couldBranch) {
                roomLayouts = roomLayouts.OrderByDescending(layout => layout.GetConnectionCount() <= 2 ? 1 : -1).ToList();

                GD.Print("Can place branch " + roomIndex);
            } else {
                roomLayouts = roomLayouts.Where(layout => layout.GetConnectionCount() <= 2).ToList();

                GD.Print("Can not place branch " + roomIndex);
            }
        }

        if (roomsToPlace == 1) roomLayouts = new List<RoomLayout>(biome.FinalRoomLayouts).OrderBy(item => _random.Randf()).ToList();

        foreach (RoomLayout roomLayout in roomLayouts) {
            List<RoomLayout.Connection> connections = roomLayout.GetConnections().ToList();
            var validConnections = connections.Where(connection => connection.Direction == new Vector2(-lastConnection.Direction.X, -lastConnection.Direction.Y));

            if (validConnections.Count() == 0) continue;

            RoomLayout.Connection targetConnection = validConnections.First();

            connections.Remove(targetConnection);

            Vector2 placeLocation = lastConnection.Location - targetConnection.Location;

            RoomPlacement placement = new RoomPlacement {
                RoomLayout = roomLayout,
                Location = new Vector2I((int)placeLocation.X, (int)placeLocation.Y)
            };

            bool valid = true;

            foreach (RoomPlacement otherRoom in placedRooms) {
                if (!otherRoom.Intersects(placement)) continue;

                valid = false;

                break;
            }

            if (!valid) continue;

            GD.Print("Found one valid placement! " + roomIndex + " " + roomLayout.ResourcePath);

            placedRooms.Push(placement);

            if (roomsToPlace == 1) return true;

            connections = connections.OrderBy(item => _random.Randf()).ToList();

            RoomLayout.Connection nextConnection = new RoomLayout.Connection {
                Location = connections[0].Location + placeLocation,
                Direction = connections[0].Direction
            };

            connections.RemoveAt(0);

            if (connections.Count > 0) {
                BranchedRoomPlacement branchedRoomPlacement = new BranchedRoomPlacement {
                    RoomLayout = placement.RoomLayout,
                    Location = placement.Location,
                    BranchRoomPlacements = new List<Stack<RoomPlacement>>()
                };

                bool branchesValid = true;

                foreach (RoomLayout.Connection localBranchConnection in connections) {
                    Stack<RoomPlacement> branchStack = new Stack<RoomPlacement>();

                    RoomLayout.Connection branchConnection = new RoomLayout.Connection {
                        Location = localBranchConnection.Location + placeLocation,
                        Direction = localBranchConnection.Direction
                    };

                    GD.Print("Branching in direction " + branchConnection.Direction);

                    bool branchResult = TryPlaceBranchRooms(biome, placedRooms, branchStack, branchConnection, _random.RandiRange(biome.BranchSize.X, biome.BranchSize.Y));

                    if (!branchResult) {
                        valid = false;

                        break;
                    }

                    branchedRoomPlacement.BranchRoomPlacements.Add(branchStack);
                }

                if (!branchesValid) {
                    GD.Print("Didn't find valid placements with branches " + roomIndex);

                    continue;
                }

                branches++;

                placedRooms.Pop();
                placedRooms.Push(branchedRoomPlacement);

                GD.Print("Found valid placements with branches " + roomIndex);
            }

            bool result = TryPlaceRooms(biome, placedRooms, nextConnection, roomsToPlace - 1, size, branches);

            if (result) return true;

            GD.Print("Couldn't find a placement! " + roomIndex);

            placedRooms.Pop();

            if (connections.Count > 0) branches--;
        }

        return false;
    }

    private bool TryPlaceBranchRooms(Biome biome, Stack<RoomPlacement> placedRooms, Stack<RoomPlacement> branchPlacedRooms, RoomLayout.Connection lastConnection, int roomsToPlace) {
        List<RoomLayout> roomLayouts = new List<RoomLayout>(biome.RoomLayouts).OrderBy(item => _random.Randf()).Where(layout => layout.GetConnectionCount() <= 2).ToList();

        if (roomsToPlace == 1) roomLayouts = new List<RoomLayout>(biome.FinalBranchRoomLayouts).OrderBy(item => _random.Randf()).ToList();

        foreach (RoomLayout roomLayout in roomLayouts) {
            List<RoomLayout.Connection> connections = roomLayout.GetConnections().ToList();
            var validConnections = connections.Where(connection => connection.Direction == new Vector2(-lastConnection.Direction.X, -lastConnection.Direction.Y));

            if (validConnections.Count() == 0) continue;

            RoomLayout.Connection targetConnection = validConnections.First();

            connections.Remove(targetConnection);

            Vector2 placeLocation = lastConnection.Location - targetConnection.Location;

            RoomPlacement placement = new RoomPlacement {
                RoomLayout = roomLayout,
                Location = new Vector2I((int)placeLocation.X, (int)placeLocation.Y)
            };

            bool valid = true;

            foreach (RoomPlacement otherRoom in placedRooms) {
                if (!otherRoom.Intersects(placement)) continue;

                valid = false;

                break;
            }

            if (!valid) continue;

            foreach (RoomPlacement otherRoom in branchPlacedRooms) {
                if (!otherRoom.Intersects(placement)) continue;

                valid = false;

                break;
            }

            if (!valid) continue;

            GD.Print("BRANCH: Found one valid placement! " + roomsToPlace + " " + roomLayout.ResourcePath);

            branchPlacedRooms.Push(placement);

            if (roomsToPlace == 1) return true;

            RoomLayout.Connection nextConnection = new RoomLayout.Connection {
                Location = connections[0].Location + placeLocation,
                Direction = connections[0].Direction
            };

            bool result = TryPlaceBranchRooms(biome, placedRooms, branchPlacedRooms, nextConnection, roomsToPlace - 1);

            if (result) return true;

            GD.Print("BRANCH: Couldn't find a placement! " + roomsToPlace);

            branchPlacedRooms.Pop();
        }

        return false;
    }

    private void PlaceRoom(RoomPlacement placement) {
        foreach (Vector2 tileLocation in placement.RoomLayout.Walls) {
            Vector2I realTileLocation = placement.Location + new Vector2I((int)tileLocation.X, (int)tileLocation.Y);

            _wallsTileMapLayer.SetCell(realTileLocation, 0, new Vector2I(3, 0));
        }

        if (placement is BranchedRoomPlacement branchPlacement) {
            foreach (Stack<RoomPlacement> branchStack in branchPlacement.BranchRoomPlacements) {
                foreach (RoomPlacement branchRoomPlacement in branchStack) {
                    foreach (Vector2 tileLocation in branchRoomPlacement.RoomLayout.Walls) {
                        Vector2I realTileLocation = branchRoomPlacement.Location + new Vector2I((int)tileLocation.X, (int)tileLocation.Y);

                        _wallsTileMapLayer.SetCell(realTileLocation, 0, new Vector2I(3, 0));
                    }
                }
            }
        }
    }
}
