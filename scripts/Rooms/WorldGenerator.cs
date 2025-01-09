using Godot;
using Networking;
using Riptide;
using System.Collections.Generic;
using System.Data;
using System.Linq;

public partial class WorldGenerator : Node2D, NetworkPointUser {
    private struct RoomPlacement {
        public RoomLayout RoomLayout;
        public Vector2I Location;

        public Vector2 GetTopLeftBound() {
            return Location + RoomLayout.TopLeftBound;
        }

        public Vector2 GetBottomRightBound() {
            return Location + RoomLayout.BottomRightBound;
        }

        public bool Intersects(RoomPlacement otherRoom) {
            if (otherRoom.GetTopLeftBound().X >= GetTopLeftBound().X && otherRoom.GetTopLeftBound().X < GetBottomRightBound().X && otherRoom.GetTopLeftBound().Y >= GetTopLeftBound().Y && otherRoom.GetTopLeftBound().Y < GetBottomRightBound().Y) return true;

            if (otherRoom.GetBottomRightBound().X > GetTopLeftBound().X && otherRoom.GetBottomRightBound().X <= GetBottomRightBound().X && otherRoom.GetTopLeftBound().Y >= GetTopLeftBound().Y && otherRoom.GetTopLeftBound().Y < GetBottomRightBound().Y) return true;

            if (otherRoom.GetTopLeftBound().X >= GetTopLeftBound().X && otherRoom.GetTopLeftBound().X < GetBottomRightBound().X && otherRoom.GetBottomRightBound().Y > GetTopLeftBound().Y && otherRoom.GetBottomRightBound().Y <= GetBottomRightBound().Y) return true;

            if (otherRoom.GetBottomRightBound().X > GetTopLeftBound().X && otherRoom.GetBottomRightBound().X <= GetBottomRightBound().X && otherRoom.GetBottomRightBound().Y > GetTopLeftBound().Y && otherRoom.GetBottomRightBound().Y <= GetBottomRightBound().Y) return true;

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

        Vector2 spawnRoomPlaceLocation = -((spawnRoomLayout.BottomRightBound - spawnRoomLayout.TopLeftBound) / 2f).Floor();
        lastConnection = spawnRoomLayout.GetConnections()[0];
        lastConnection.Location += spawnRoomPlaceLocation;

        RoomPlacement spawnRoomPlacement = new RoomPlacement {
            RoomLayout = spawnRoomLayout,
            Location = new Vector2I((int)spawnRoomPlaceLocation.X, (int)spawnRoomPlaceLocation.Y)
        };

        Stack<RoomPlacement> placedRooms = new Stack<RoomPlacement>();
        placedRooms.Push(spawnRoomPlacement);

        bool result = TryPlaceRooms(Biomes[0], placedRooms, lastConnection, 6);

        GD.Print(result);

        foreach (RoomPlacement placement in placedRooms) {
            PlaceRoom(placement);
        }

        // for (int index = 0; index < 5; index++) {
        //     RoomLayout roomLayout = Biomes[0].GetRoomLayout(Game.RandomNumberGenerator.RandiRange(0, Biomes[0].Rooms.Length - 1));

        //     List<RoomLayout.Connection> connections = roomLayout.GetConnections().ToList();
        //     RoomLayout.Connection targetConnection = connections.Where(connection => connection.Direction == new Vector2(-lastConnection.Direction.X, -lastConnection.Direction.Y)).First();
        //     connections.Remove(targetConnection);

        //     Vector2 placeLocation = lastConnection.Location - targetConnection.Location;

        //     PlaceRoomLayout(roomLayout, new Vector2I((int)placeLocation.X, (int)placeLocation.Y));

        //     lastConnection = connections[0];
        //     lastConnection.Location += placeLocation;
        // }
    }

    private bool TryPlaceRooms(Biome biome, Stack<RoomPlacement> placedRooms, RoomLayout.Connection lastConnection, int roomsToPlace) {
        List<RoomLayout> roomLayouts = new List<RoomLayout>(biome.RoomLayouts).OrderBy(item => _random.Randf()).ToList();

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

            GD.Print("Found one valid placement! " + roomsToPlace);

            placedRooms.Push(placement);

            RoomLayout.Connection nextConnection = new RoomLayout.Connection {
                Location = connections[0].Location + placeLocation,
                Direction = connections[0].Direction
            };

            if (roomsToPlace == 1) return true;

            bool result = TryPlaceRooms(biome, placedRooms, nextConnection, roomsToPlace - 1);

            if (result) return true;

            GD.Print("Couldn't find a placement! " + roomsToPlace);

            placedRooms.Pop();
        }

        return false;
    }

    private void PlaceRoom(RoomPlacement placement) {
        foreach (Vector2 tileLocation in placement.RoomLayout.Walls) {
            Vector2I realTileLocation = placement.Location + new Vector2I((int)tileLocation.X, (int)tileLocation.Y) - new Vector2I((int)placement.RoomLayout.TopLeftBound.X, (int)placement.RoomLayout.TopLeftBound.Y);

            _wallsTileMapLayer.SetCell(realTileLocation, 0, new Vector2I(3, 0));
        }
    }
}
