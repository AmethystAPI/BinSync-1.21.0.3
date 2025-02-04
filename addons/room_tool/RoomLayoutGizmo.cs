using Godot;
using System.Collections.Generic;

[Tool]
public partial class RoomLayoutGizmo : Node2D {
    public HashSet<RoomLayout.Connection> Connections = new HashSet<RoomLayout.Connection>();
    public List<Vector2> Bounds = new List<Vector2>();
    public Vector2 Direction = Vector2.Right;
    public List<Vector2> SpawnLocations = new List<Vector2>();
    public List<Vector2> EdgeLocations = new List<Vector2>();
    public List<int> EdgeDistances = new List<int>();

    public override void _Process(double delta) {
        QueueRedraw();
    }

    public override void _Draw() {
        ZIndex = 100;

        DrawCircle((GetGlobalMousePosition() / 16f).Round() * 16f, 2f, new Color(0f, 0f, 0f, 0.5f));
        DrawLine((GetGlobalMousePosition() / 16f).Round() * 16f, ((GetGlobalMousePosition() / 16f).Round() + Direction) * 16f, new Color(0f, 0f, 0f, 0.5f), 2f);

        foreach (RoomLayout.Connection connection in Connections) {
            DrawCircle(connection.Location * 16f, 2f, new Color("green"));
            DrawLine(connection.Location * 16f, (connection.Location + connection.Direction) * 16f, new Color("green"), 2f);
        }

        foreach (Vector2 bound in Bounds) {
            DrawCircle(bound * 16f, 2f, new Color("orange"));
        }

        foreach (Vector2 spawnLocation in SpawnLocations) {
            DrawRect(new Rect2(spawnLocation * 16f, 16f, 16f), new Color(0, 0, 1, 0.2f));
        }

        for(int index = 0; index < EdgeLocations.Count; index++) {
            DrawRect(new Rect2(EdgeLocations[index] * 16f, 16f, 16f), new Color(0, 1, 0, 0.8f * (1f - EdgeDistances[index] / 20f)));
        }
    }
}
