using System;
using Godot;

[Tool]
public partial class SmartTileset : Resource {
    [ExportGroup("Roofs")]
    [Export] public Vector2[] RoofsTopLeft = new Vector2[0];
    [Export] public Vector2[] RoofsTop = new Vector2[0];
    [Export] public Vector2[] RoofsTopRight = new Vector2[0];
    [Export] public Vector2[] RoofsLeft = new Vector2[0];
    [Export] public Vector2[] RoofsMiddle = new Vector2[0];
    [Export] public Vector2[] RoofsRight = new Vector2[0];
    [Export] public Vector2[] RoofsBottomLeft = new Vector2[0];
    [Export] public Vector2[] RoofsBottom = new Vector2[0];
    [Export] public Vector2[] RoofsBottomRight = new Vector2[0];
    [Export] public Vector2[] RoofsInsideTopLeft = new Vector2[0];
    [Export] public Vector2[] RoofsInsideTopRight = new Vector2[0];
    [Export] public Vector2[] RoofsInsideBottomLeft = new Vector2[0];
    [Export] public Vector2[] RoofsInsideBottomRight = new Vector2[0];

    [ExportGroup("Walls")]
    [Export] public Vector2[] WallsLeft = new Vector2[0];
    [Export] public Vector2[] WallsMiddle = new Vector2[0];
    [Export] public Vector2[] WallsRight = new Vector2[0];
    [Export] public Vector2[] WallsInsideLeft = new Vector2[0];
    [Export] public Vector2[] WallsInsideRight = new Vector2[0];
    [Export] public Vector2 WallHidden;

    [ExportGroup("Shadows")]
    [Export] public Vector2 ShadowLeft;
    [Export] public Vector2 ShadowMiddle;
    [Export] public Vector2 ShadowRight;
    [Export] public Vector2 ShadowInsideLeft;
    [Export] public Vector2 ShadowInsideRight;

    [ExportGroup("Floors")]
    [Export] public Vector2 Floor;

    public Vector2 GetRandom(Vector2[] locations) {
        return locations[new RandomNumberGenerator().RandiRange(0, locations.Length - 1)];
    }
}