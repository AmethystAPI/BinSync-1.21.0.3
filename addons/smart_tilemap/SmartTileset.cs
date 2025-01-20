using System;
using Godot;

[Tool]
public partial class SmartTileset : Resource {
    public struct Tile {
        public int Source;
        public Vector2I Location;
    }

    [Export] public int SourceId = 0;

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

    private Vector2 GetWallTileLocation(Vector2I location, Func<Vector2I, bool> isWallTile) {
        bool right = isWallTile(location + Vector2I.Right);
        bool left = isWallTile(location + Vector2I.Left);
        bool down = isWallTile(location + Vector2I.Down);

        bool downRight = isWallTile(location + Vector2I.Down + Vector2I.Right);
        bool downLeft = isWallTile(location + Vector2I.Down + Vector2I.Left);

        bool downDown = isWallTile(location + Vector2I.Down * 2);

        if (down && !downRight && right) return GetRandom(WallsInsideLeft);
        if (down && !downLeft && left) return GetRandom(WallsInsideRight);

        if (down && !downDown && !downRight) return GetRandom(WallsRight) + Vector2.Up;
        if (down && !downDown && !downLeft) return GetRandom(WallsLeft) + Vector2.Up;
        if (down && !downDown) return GetRandom(WallsMiddle) + Vector2.Up;

        if (!down && !right) return GetRandom(WallsRight);
        if (!down && !left) return GetRandom(WallsLeft);
        if (!down) return GetRandom(WallsMiddle);

        return WallHidden;
    }

    public Tile GetWallTile(Vector2I location, Func<Vector2I, bool> isWallTile) {
        return new Tile {
            Location = (Vector2I)GetWallTileLocation(location, isWallTile),
            Source = SourceId,
        };
    }

    private bool IsTileRoofOrBounds(Vector2I location, Func<Vector2I, bool> isWallTile) {
        return isWallTile(location) && isWallTile(location + Vector2I.Down);
    }

    private Vector2? GetRoofTileLocation(Vector2I location, Func<Vector2I, bool> isWallTile) {
        if (!isWallTile(location + Vector2I.Down)) return null;

        bool right = IsTileRoofOrBounds(location + Vector2I.Right, isWallTile);
        bool left = IsTileRoofOrBounds(location + Vector2I.Left, isWallTile);
        bool up = IsTileRoofOrBounds(location + Vector2I.Up, isWallTile);
        bool down = IsTileRoofOrBounds(location + Vector2I.Down, isWallTile);

        bool upRight = IsTileRoofOrBounds(location + Vector2I.Up + Vector2I.Right, isWallTile);
        bool upLeft = IsTileRoofOrBounds(location + Vector2I.Up + Vector2I.Left, isWallTile);
        bool downRight = IsTileRoofOrBounds(location + Vector2I.Down + Vector2I.Right, isWallTile);
        bool downLeft = IsTileRoofOrBounds(location + Vector2I.Down + Vector2I.Left, isWallTile);

        if (left && up && !upLeft) return GetRandom(RoofsInsideBottomRight);
        if (!left && !up && !upLeft) return GetRandom(RoofsTopLeft);

        if (right && up && !upRight) return GetRandom(RoofsInsideBottomLeft);
        if (!right && !up && !upRight) return GetRandom(RoofsTopRight);

        if (left && down && !downLeft) return GetRandom(RoofsInsideTopRight);
        if (!left && !down && !downLeft) return GetRandom(RoofsBottomLeft);

        if (right && down && !downRight) return GetRandom(RoofsInsideTopLeft);
        if (!right && !down && !downRight) return GetRandom(RoofsBottomRight);

        if (!right) return GetRandom(RoofsRight);
        if (!left) return GetRandom(RoofsLeft);
        if (!up) return GetRandom(RoofsTop);
        if (!down) return GetRandom(RoofsBottom);

        return GetRandom(RoofsMiddle);
    }

    public Tile? GetRoofTile(Vector2I location, Func<Vector2I, bool> isWallTile) {
        Vector2? tile = GetRoofTileLocation(location, isWallTile);

        if (tile == null) return null;

        return new Tile {
            Location = (Vector2I)tile,
            Source = SourceId,
        };
    }

    private (Vector2?, Vector2?) GetShadowTileLocation(Vector2I location, Func<Vector2I, bool> isWallTile) {
        bool right = isWallTile(location + Vector2I.Right);
        bool left = isWallTile(location + Vector2I.Left);
        bool down = isWallTile(location + Vector2I.Down);

        bool downRight = isWallTile(location + Vector2I.Down + Vector2I.Right);
        bool downLeft = isWallTile(location + Vector2I.Down + Vector2I.Left);

        bool downDown = isWallTile(location + Vector2I.Down * 2);

        if (down && !downRight && right) return (ShadowInsideLeft, ShadowMiddle + Vector2.Up);
        if (down && !downLeft && left) return (ShadowInsideRight, ShadowMiddle + Vector2.Up);

        if (down && !downDown && !downRight) return (ShadowRight + Vector2.Up, null);
        if (down && !downDown && !downLeft) return (ShadowLeft + Vector2.Up, null);
        if (down && !downDown) return (ShadowMiddle + Vector2.Up, null);

        if (!down && !right) return (ShadowRight, null);
        if (!down && !left) return (ShadowLeft, null);
        if (!down) return (ShadowMiddle, null);

        return (null, null);
    }

    public (Tile?, Tile?) GetShadowTile(Vector2I location, Func<Vector2I, bool> isWallTile) {
        (Vector2? possibleTile, Vector2? possibleUpperTile) = GetShadowTileLocation(location, isWallTile);

        if (possibleTile is Vector2 tile) {
            if (possibleUpperTile is Vector2 upperTile) {
                return (new Tile {
                    Location = (Vector2I)tile,
                    Source = SourceId,
                }, new Tile {
                    Location = (Vector2I)upperTile,
                    Source = SourceId,
                });
            } else {
                return (new Tile {
                    Location = (Vector2I)tile,
                    Source = SourceId,
                }, null);
            }
        }

        return (null, null);
    }
}