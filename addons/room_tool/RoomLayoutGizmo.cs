using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class RoomLayoutGizmo : Node2D {
    public HashSet<Vector2> Connections = new HashSet<Vector2>();
    public List<Vector2> Bounds = new List<Vector2>();

    public override void _Process(double delta) {
        QueueRedraw();
    }

    public override void _Draw() {
        DrawCircle((GetGlobalMousePosition() / 16f).Round() * 16f, 2f, new Color(0f, 0f, 0f, 0.5f));

        foreach (Vector2 connection in Connections) {
            DrawCircle(connection, 2f, new Color("green"));
        }

        foreach (Vector2 bound in Bounds) {
            DrawCircle(bound, 2f, new Color("orange"));
        }
    }
}
