using Godot;

[Tool]
public partial class RoomLayout : Resource {
    public struct Connection {
        public Vector2 Location;
        public Vector2 Direction;
    }

    [Export] public Vector2[] ConnectionLocations;
    [Export] public Vector2[] ConnectionDirections;
    [Export] public Vector2[] Walls;
    [Export] public Vector2 TopLeftBound;
    [Export] public Vector2 BottomRightBound;
    [Export] public Vector2[] SpawnLocations;
    [Export] public Vector2[] EdgeFieldPosition;
    [Export] public int[] EdgeFieldDistance;

    public Connection[] GetConnections() {
        Connection[] connections = new Connection[ConnectionLocations == null ? 0 : ConnectionLocations.Length];

        for (int index = 0; index < connections.Length; index++) {
            connections[index] = new Connection {
                Location = ConnectionLocations[index],
                Direction = ConnectionDirections[index]
            };
        }

        return connections;
    }

    public int GetConnectionCount() {
        return ConnectionLocations.Length;
    }

    public void SetConnections(Connection[] connections) {
        ConnectionLocations = new Vector2[connections.Length];
        ConnectionDirections = new Vector2[connections.Length];

        for (int index = 0; index < connections.Length; index++) {
            ConnectionLocations[index] = connections[index].Location;
            ConnectionDirections[index] = connections[index].Direction;
        }
    }
}