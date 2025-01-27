using System;
using System.Collections.Generic;
using Godot;

public class Decoration {
    public Func<WorldGenerator.RoomPlacement, List<Vector2I>, List<WorldGenerator.DecorationPlacement>> Generate;
}