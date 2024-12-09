using Godot;
using System.Collections.Generic;

[Tool]
public partial class RoomLayoutGizmo : Node2D {
    public HashSet<RoomLayout.Connection> Connections = new HashSet<RoomLayout.Connection>();
    public List<Vector2> Bounds = new List<Vector2>();
    public Vector2 Direction = Vector2.Right;

    public override void _Process(double delta) {
        QueueRedraw();
    }

    public override void _Draw() {
        DrawCircle((GetGlobalMousePosition() / 16f).Round() * 16f, 2f, new Color(0f, 0f, 0f, 0.5f));
        DrawLine((GetGlobalMousePosition() / 16f).Round() * 16f, ((GetGlobalMousePosition() / 16f).Round() + Direction) * 16f, new Color(0f, 0f, 0f, 0.5f), 2f);

        foreach (RoomLayout.Connection connection in Connections) {
            DrawCircle(connection.Location * 16f, 2f, new Color("green"));
            DrawLine(connection.Location * 16f, (connection.Location + connection.Direction) * 16f, new Color("green"), 2f);
        }

        foreach (Vector2 bound in Bounds) {
            DrawCircle(bound * 16f, 2f, new Color("orange"));
        }
    }
}
