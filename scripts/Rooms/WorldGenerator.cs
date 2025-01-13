using Godot;
using Networking;
using System.Collections.Generic;
using System.Data;
using System.Linq;

public partial class WorldGenerator : Node, NetworkPointUser {
    public class RoomPlacement {
        public RoomLayout RoomLayout;
        public Vector2I Location;

        public virtual bool Intersects(RoomPlacement otherRoom) {
            if (otherRoom.GetTopLeftBound().X >= GetTopLeftBound().X && otherRoom.GetTopLeftBound().X < GetBottomRightBound().X && otherRoom.GetTopLeftBound().Y >= GetTopLeftBound().Y && otherRoom.GetTopLeftBound().Y < GetBottomRightBound().Y) return true;

            if (otherRoom.GetBottomRightBound().X > GetTopLeftBound().X && otherRoom.GetBottomRightBound().X <= GetBottomRightBound().X && otherRoom.GetTopLeftBound().Y >= GetTopLeftBound().Y && otherRoom.GetTopLeftBound().Y < GetBottomRightBound().Y) return true;

            if (otherRoom.GetTopLeftBound().X >= GetTopLeftBound().X && otherRoom.GetTopLeftBound().X < GetBottomRightBound().X && otherRoom.GetBottomRightBound().Y > GetTopLeftBound().Y && otherRoom.GetBottomRightBound().Y <= GetBottomRightBound().Y) return true;

            if (otherRoom.GetBottomRightBound().X > GetTopLeftBound().X && otherRoom.GetBottomRightBound().X <= GetBottomRightBound().X && otherRoom.GetBottomRightBound().Y > GetTopLeftBound().Y && otherRoom.GetBottomRightBound().Y <= GetBottomRightBound().Y) return true;


            if (GetTopLeftBound().X >= otherRoom.GetTopLeftBound().X && GetTopLeftBound().X < otherRoom.GetBottomRightBound().X && GetTopLeftBound().Y >= otherRoom.GetTopLeftBound().Y && GetTopLeftBound().Y < otherRoom.GetBottomRightBound().Y) return true;

            if (GetBottomRightBound().X > otherRoom.GetTopLeftBound().X && GetBottomRightBound().X <= otherRoom.GetBottomRightBound().X && GetTopLeftBound().Y >= otherRoom.GetTopLeftBound().Y && GetTopLeftBound().Y < otherRoom.GetBottomRightBound().Y) return true;

            if (GetTopLeftBound().X >= otherRoom.GetTopLeftBound().X && GetTopLeftBound().X < otherRoom.GetBottomRightBound().X && GetBottomRightBound().Y > otherRoom.GetTopLeftBound().Y && GetBottomRightBound().Y <= otherRoom.GetBottomRightBound().Y) return true;

            if (GetBottomRightBound().X > otherRoom.GetTopLeftBound().X && GetBottomRightBound().X <= otherRoom.GetBottomRightBound().X && GetBottomRightBound().Y > otherRoom.GetTopLeftBound().Y && GetBottomRightBound().Y <= otherRoom.GetBottomRightBound().Y) return true;


            return false;
        }

        public Vector2 GetTopLeftBound() {
            return Location + RoomLayout.TopLeftBound;
        }

        public Vector2 GetBottomRightBound() {
            return Location + RoomLayout.BottomRightBound;
        }
    }

    public class BranchedRoomPlacement : RoomPlacement {
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

    public static WorldGenerator Me;

    public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

    private RandomNumberGenerator _random;

    public override void _Ready() {
        Me = this;

        NetworkPoint.Setup(this);
    }

    public Stack<RoomPlacement> Generate(ulong seed, Biome biome) {
        _random = new RandomNumberGenerator();
        _random.Seed = seed;

        RoomLayout.Connection lastConnection;

        RoomLayout spawnRoomLayout = biome.SpawnRoomLayouts[0];

        Vector2 spawnRoomPlaceLocation = Vector2.Zero;
        lastConnection = spawnRoomLayout.GetConnections()[0];
        lastConnection.Location += spawnRoomPlaceLocation;

        RoomPlacement spawnRoomPlacement = new RoomPlacement {
            RoomLayout = spawnRoomLayout,
            Location = new Vector2I((int)spawnRoomPlaceLocation.X, (int)spawnRoomPlaceLocation.Y)
        };

        Stack<RoomPlacement> placedRooms = new Stack<RoomPlacement>();
        placedRooms.Push(spawnRoomPlacement);

        int size = _random.RandiRange(biome.Size.X, biome.Size.Y);

        bool result = TryPlaceRooms(biome, placedRooms, lastConnection, size - 1, size, 0);

        if (!result) return Generate(seed + 1, biome);

        return placedRooms;
    }

    private bool TryPlaceRooms(Biome biome, Stack<RoomPlacement> placedRooms, RoomLayout.Connection lastConnection, int roomsToPlace, int size, int branches) {
        List<RoomLayout> roomLayouts = new List<RoomLayout>(biome.RoomLayouts).OrderBy(item => _random.Randf()).ToList();

        int roomIndex = size - roomsToPlace;

        // GD.Print("=============================== " + roomIndex);

        bool mustBranch = branches < biome.BranchRanges.Length && biome.BranchRanges.Any(range => range.Y == roomIndex);

        if (mustBranch) {
            roomLayouts = roomLayouts.Where(layout => layout.GetConnectionCount() > 2).ToList();

            // GD.Print("Must place branch " + roomIndex);
        } else {
            bool couldBranch = branches < biome.BranchRanges.Length && biome.BranchRanges[branches..biome.BranchRanges.Length].Any(range => roomIndex >= range.X);

            if (couldBranch) {
                roomLayouts = roomLayouts.OrderByDescending(layout => layout.GetConnectionCount() <= 2 ? 1 : -1).ToList();

                // GD.Print("Can place branch " + roomIndex);
            } else {
                roomLayouts = roomLayouts.Where(layout => layout.GetConnectionCount() <= 2).ToList();

                // GD.Print("Can not place branch " + roomIndex);
            }
        }

        if (roomsToPlace == 1) roomLayouts = new List<RoomLayout>(biome.FinalRoomLayouts).OrderBy(item => _random.Randf()).ToList();

        foreach (RoomLayout roomLayout in roomLayouts) {
            // GD.Print("---------- " + roomLayout.ResourcePath.Substring("res://generated/rooms/".Length));

            List<RoomLayout.Connection> connections = roomLayout.GetConnections().ToList();
            var validConnections = connections.Where(connection => connection.Direction == new Vector2(-lastConnection.Direction.X, -lastConnection.Direction.Y));

            if (validConnections.Count() == 0) {
                // GD.Print("NO VALID CONNECTIONS!");

                continue;
            }

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

            if (!valid) {
                // GD.Print("COLLIDES!");

                continue;
            }

            // GD.Print("Found one valid placement!");

            placedRooms.Push(placement);

            if (roomsToPlace == 1) return true;

            connections = connections.OrderBy(item => _random.Randf()).ToList();

            RoomLayout.Connection nextConnection = new RoomLayout.Connection {
                Location = connections[0].Location + placeLocation,
                Direction = connections[0].Direction
            };

            if (connections.Count > 1) {
                bool nextConnectionValid = false;

                for (int nextConnectionIndex = 0; nextConnectionIndex < connections.Count; nextConnectionIndex++) {
                    // GD.Print("Attempting next connection index " + nextConnection);

                    nextConnection = new RoomLayout.Connection {
                        Location = connections[nextConnectionIndex].Location + placeLocation,
                        Direction = connections[nextConnectionIndex].Direction
                    };

                    BranchedRoomPlacement branchedRoomPlacement = new BranchedRoomPlacement {
                        RoomLayout = placement.RoomLayout,
                        Location = placement.Location,
                        BranchRoomPlacements = new List<Stack<RoomPlacement>>()
                    };

                    bool branchesValid = true;

                    for (int localBranchConnectionIndex = 0; localBranchConnectionIndex < connections.Count; localBranchConnectionIndex++) {
                        if (localBranchConnectionIndex == nextConnectionIndex) continue;

                        RoomLayout.Connection localBranchConnection = connections[localBranchConnectionIndex];

                        Stack<RoomPlacement> branchStack = new Stack<RoomPlacement>();

                        RoomLayout.Connection branchConnection = new RoomLayout.Connection {
                            Location = localBranchConnection.Location + placeLocation,
                            Direction = localBranchConnection.Direction
                        };

                        // GD.Print("Branching in direction " + branchConnection.Direction);

                        bool branchResult = TryPlaceBranchRooms(biome, placedRooms, branchStack, branchConnection, _random.RandiRange(biome.BranchSize.X, biome.BranchSize.Y));

                        // GD.Print("Got branch result " + branchResult);

                        if (!branchResult) {
                            branchesValid = false;

                            break;
                        }

                        branchedRoomPlacement.BranchRoomPlacements.Add(branchStack);
                    }

                    // GD.Print("Branches valid " + branchesValid);

                    if (!branchesValid) {
                        // GD.Print("NO VALID BRANCHES");

                        continue;
                    }

                    nextConnectionValid = true;

                    branches++;

                    placedRooms.Pop();
                    placedRooms.Push(branchedRoomPlacement);

                    // GD.Print("Found valid placements with branches");

                    break;
                }

                if (!nextConnectionValid) {
                    // GD.Print("NO VALID BRANCHES IN ALL CONNECTIONS");

                    placedRooms.Pop();

                    continue;
                }
            }

            bool result = TryPlaceRooms(biome, placedRooms, nextConnection, roomsToPlace - 1, size, branches);

            if (result) return true;

            if (connections.Count > 1) branches--;

            placedRooms.Pop();

            // GD.Print("xxxxxxxxxxxxxxx Couldn't find a placement! " + roomIndex + " " + branches);
        }

        return false;
    }

    private bool TryPlaceBranchRooms(Biome biome, Stack<RoomPlacement> placedRooms, Stack<RoomPlacement> branchPlacedRooms, RoomLayout.Connection lastConnection, int roomsToPlace) {
        List<RoomLayout> roomLayouts = new List<RoomLayout>(biome.RoomLayouts).OrderBy(item => _random.Randf()).Where(layout => layout.GetConnectionCount() <= 2).ToList();

        if (roomsToPlace == 1) roomLayouts = new List<RoomLayout>(biome.FinalBranchRoomLayouts).OrderBy(item => _random.Randf()).ToList();

        // GD.Print("BRANCH: =============================== " + roomsToPlace);

        foreach (RoomLayout roomLayout in roomLayouts) {
            // GD.Print("BRANCH: ---------- " + roomLayout.ResourcePath.Substring("res://generated/rooms/".Length));

            List<RoomLayout.Connection> connections = roomLayout.GetConnections().ToList();
            var validConnections = connections.Where(connection => connection.Direction == new Vector2(-lastConnection.Direction.X, -lastConnection.Direction.Y));

            if (validConnections.Count() == 0) {
                // GD.Print("BRANCH: NO VALID CONNECTIONS!");

                continue;
            }

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

            if (!valid) {
                // GD.Print("BRANCH: COLLIDES MAIN!");

                continue;
            }

            foreach (RoomPlacement otherRoom in branchPlacedRooms) {
                if (!otherRoom.Intersects(placement)) continue;

                valid = false;

                break;
            }

            if (!valid) {
                // GD.Print("BRANCH: COLLIDES BRANCH!");

                continue;
            }

            // GD.Print("BRANCH: Found one valid placement!");

            branchPlacedRooms.Push(placement);

            if (roomsToPlace == 1) return true;

            RoomLayout.Connection nextConnection = new RoomLayout.Connection {
                Location = connections[0].Location + placeLocation,
                Direction = connections[0].Direction
            };

            bool result = TryPlaceBranchRooms(biome, placedRooms, branchPlacedRooms, nextConnection, roomsToPlace - 1);

            if (result) return true;

            // GD.Print("BRANCH: xxxxxxxxxxxxxxx Couldn't find a placement! " + roomsToPlace);

            branchPlacedRooms.Pop();
        }

        // GD.Print("BRANCH: xxxxxxxxxxxxxxx Exiting " + roomsToPlace + " with fail");

        return false;
    }


}
