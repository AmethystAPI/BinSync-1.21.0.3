using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[Tool]
public partial class BorderTool : Node2D {
    [Export] public Border Border;
    [Export] public TileMap TileMap;

    private bool _justPressedGenerate = false;
    private TileMapLayer _screen;
    private Node2D _paralaxOrigin1;
    private Node2D _paralaxOrigin2;
    private HashSet<Vector2I> _edgeTiles = new HashSet<Vector2I>();
    private HashSet<Vector2I> _finalEdgeTiles = new HashSet<Vector2I>();
    private List<List<Vector2>> _splines = new List<List<Vector2>>();

#if TOOLS
    public override void _Process(double delta) {
        if (Input.IsKeyPressed(Key.Backslash) && _justPressedGenerate) return;

        if (!Input.IsKeyPressed(Key.Backslash)) {
            _justPressedGenerate = false;

            return;
        }

        _justPressedGenerate = true;

        Generate();
    }

    private void Generate() {
        GD.Print("Placing border!");

        _screen = GetParent().GetNode<TileMapLayer>("Paralax2/Origin/Screen");
        _screen.Clear();

        _paralaxOrigin1 = GetParent().GetNode<Node2D>("Paralax/Origin");
        _paralaxOrigin2 = GetParent().GetNode<Node2D>("Paralax2/Origin");

        foreach (Node child in GetChildren()) {
            child.QueueFree();
        }

        foreach (Node child in _paralaxOrigin1.GetChildren()) {
            if (child.Name == "Screen") continue;

            child.QueueFree();
        }

        foreach (Node child in _paralaxOrigin2.GetChildren()) {
            if (child.Name == "Screen") continue;

            child.QueueFree();
        }

        BuildEdges();

        PlaceDecorations();

        PlaceScreen();

        QueueRedraw();
    }

    private void BuildEdges() {
        _edgeTiles.Clear();
        _finalEdgeTiles.Clear();
        _splines.Clear();

        Rect2I rect = TileMap.GetUsedRect();

        int wallLayer = 0;

        for (int index = 1; index < TileMap.GetLayersCount(); index++) {
            if (TileMap.GetLayerName(index) == "Walls") {
                wallLayer = index;

                break;
            }
        }

        for (int x = rect.Position.X; x < rect.End.X; x++) {
            for (int y = rect.Position.Y; y < rect.End.Y; y++) {
                Vector2I position = new Vector2I(x, y);

                if (TileMap.GetCellTileData(wallLayer, position) == null) continue;

                bool emptyLeft = rect.HasPoint(position + Vector2I.Left) && TileMap.GetCellTileData(wallLayer, position + Vector2I.Left) == null;
                bool emptyRight = rect.HasPoint(position + Vector2I.Right) && TileMap.GetCellTileData(wallLayer, position + Vector2I.Right) == null;
                bool emptyUp = rect.HasPoint(position + Vector2I.Up) && TileMap.GetCellTileData(wallLayer, position + Vector2I.Up) == null;
                bool emptyDown = rect.HasPoint(position + Vector2I.Down) && TileMap.GetCellTileData(wallLayer, position + Vector2I.Down) == null;

                bool edge = emptyLeft || emptyRight | emptyUp || emptyDown;

                if (!edge) continue;

                _edgeTiles.Add(position);
            }
        }

        for (int iteration = 0; iteration < 2; iteration++) {
            HashSet<Vector2I> expandPositions = new HashSet<Vector2I>();

            foreach (Vector2I position in _edgeTiles) {
                if (!expandPositions.Contains(position + Vector2I.Left)) expandPositions.Add(position + Vector2I.Left);
                if (!expandPositions.Contains(position + Vector2I.Right)) expandPositions.Add(position + Vector2I.Right);
                if (!expandPositions.Contains(position + Vector2I.Up)) expandPositions.Add(position + Vector2I.Up);
                if (!expandPositions.Contains(position + Vector2I.Down)) expandPositions.Add(position + Vector2I.Down);
            }

            foreach (Vector2I position in expandPositions) {
                if (_edgeTiles.Contains(position)) continue;
                if (!rect.HasPoint(position)) continue;
                if (TileMap.GetCellTileData(wallLayer, position) == null) continue;

                _edgeTiles.Add(position);

                if (iteration == 1) {
                    _finalEdgeTiles.Add(position);
                }
            }
        }

        HashSet<Vector2I> unusedPositions = new HashSet<Vector2I>(_finalEdgeTiles);

        List<Vector2> buildingSpline = new List<Vector2>();
        List<Vector2I> positionQueue = new List<Vector2I>();

        Vector2I[] possibleConnectedOffsets = new Vector2I[] { Vector2I.Up + Vector2I.Left, Vector2I.Up, Vector2I.Up + Vector2I.Right, Vector2I.Left, Vector2I.Right, Vector2I.Down + Vector2I.Left, Vector2I.Down, Vector2I.Down + Vector2I.Right };

        int limitA = 0;

        while (unusedPositions.Count > 0 && limitA < 10) {
            Vector2I startPosition = unusedPositions.Where(position => {
                int connected = 0;

                foreach (Vector2I offset in possibleConnectedOffsets) {
                    if (unusedPositions.Contains(position + offset)) connected++;
                }

                return connected == 1;
            }).First();

            unusedPositions.Remove(startPosition);

            buildingSpline.Add(startPosition);
            positionQueue.Add(startPosition);

            int limitB = 0;

            while (positionQueue.Count > 0 && limitB < 100) {
                Vector2I currentPosition = positionQueue[0];
                positionQueue.RemoveAt(0);

                foreach (Vector2I offset in possibleConnectedOffsets) {
                    if (unusedPositions.Contains(currentPosition + offset)) {
                        unusedPositions.Remove(currentPosition + offset);
                        buildingSpline.Add(currentPosition + offset);
                        positionQueue.Add(currentPosition + offset);

                        break;
                    }
                }

                limitB++;
            }

            if (limitB == 100) GD.Print("FAILED: limitB");

            _splines.Add(buildingSpline);

            buildingSpline = new List<Vector2>();

            limitA++;
        }

        if (limitA == 10) GD.Print("FAILED: limitA");

        foreach (List<Vector2> spline in _splines) {
            for (int index = 0; index < spline.Count; index++) {
                spline[index] *= 1.126f;
            }
        }
    }

    private void PlaceDecorations() {
        RandomNumberGenerator random = new RandomNumberGenerator();

        for (int decorationIndex = 0; decorationIndex < Border.Textures.Length; decorationIndex++) {
            Texture2D texture = Border.Textures[decorationIndex];
            float spacing = Border.Spacings[decorationIndex];
            Vector2 variance = Border.Variances[decorationIndex];
            float offset = Border.Offsets[decorationIndex];
            int layer = Border.Layers[decorationIndex];

            foreach (List<Vector2> spline in _splines) {
                float spacingTillNextDecoration = offset;

                for (int nodeIndex = 1; nodeIndex < spline.Count; nodeIndex++) {
                    Vector2 previousNode = spline[nodeIndex - 1] * 16f + Vector2.One * 8f;
                    Vector2 nextNode = spline[nodeIndex] * 16f + Vector2.One * 8f;

                    float length = previousNode.DistanceTo(nextNode);

                    spacingTillNextDecoration -= length;

                    if (spacingTillNextDecoration > 0f) continue;

                    float factor = 1f + spacingTillNextDecoration / spacing;

                    Sprite2D sprite = new Sprite2D() {
                        Texture = texture
                    };

                    if (layer == 1) {
                        _paralaxOrigin2.AddChild(sprite);
                    } else {
                        _paralaxOrigin1.AddChild(sprite);
                    }

                    Vector2 position = previousNode.Lerp(nextNode, factor);

                    Vector2 direction = (nextNode - previousNode).Normalized();
                    Vector2 normal = new Vector2(-direction.Y, direction.X);

                    Vector2 directionToOrigin = (-position).Normalized();

                    if (normal.Dot(direction) >= 0) normal = normal.Rotated(Mathf.Pi);

                    float varianceOffset = random.RandfRange(variance.X, variance.Y);

                    sprite.Position = position + normal * varianceOffset;
                    sprite.Owner = GetOwner();
                    sprite.Rotate(random.RandiRange(0, 3) * Mathf.Pi / 2f);

                    spacingTillNextDecoration += spacing;
                }
            }
        }
    }

    private void PlaceScreen() {
        Rect2I rect = TileMap.GetUsedRect();

        int newLeft = Mathf.FloorToInt(rect.Position.X * 1.126f) - 1;
        int newRight = Mathf.FloorToInt(rect.End.X * 1.126f) + 1;
        int newTop = Mathf.FloorToInt(rect.Position.Y * 1.126f) - 1;
        int newBottom = Mathf.FloorToInt(rect.End.Y * 1.126f) + 1;

        rect.Position = new Vector2I(newLeft, newTop);
        rect.End = new Vector2I(newRight, newBottom);

        for (int x = rect.Position.X; x < rect.End.X; x++) {
            for (int y = rect.Position.Y; y < rect.End.Y; y++) {
                Vector2I position = new Vector2I(x, y);

                Vector2 rayStart = new Vector2(position.X, position.Y) * 16f + Vector2.One * 8f;
                Vector2 rayEnd = Vector2.Zero;

                bool collided = false;

                foreach (List<Vector2> spline in _splines) {
                    for (int nodeIndex = 1; nodeIndex < spline.Count; nodeIndex++) {
                        Vector2 previousNode = spline[nodeIndex - 1] * 16f + Vector2.One * 8f;
                        Vector2 nextNode = spline[nodeIndex] * 16f + Vector2.One * 8f;

                        if (LinesIntersect(rayStart, rayEnd, previousNode, nextNode)) {
                            collided = true;

                            break;
                        }
                    }
                }

                if (collided) _screen.SetCell(position, Border.Tile.X, new Vector2I(Border.Tile.Y, Border.Tile.Z));
            }
        }
    }

    public override void _Draw() {
        // foreach (Vector2I position in _finalEdgeTiles) {
        //     DrawCircle(new Vector2(position.X, position.Y) * 16f + Vector2.One * 8f, 4f, new Color("#00ff00"), true);
        // }

        foreach (List<Vector2> spline in _splines) {
            Vector2 lastPosition = Vector2.Zero;
            bool hasLast = false;

            foreach (Vector2 position in spline) {
                // DrawCircle(new Vector2(position.X, position.Y) * 16f + Vector2.One * 8f, 4f, new Color("#0000ff"), true);

                if (hasLast) {
                    DrawLine(new Vector2(lastPosition.X, lastPosition.Y) * 16f + Vector2.One * 8f, new Vector2(position.X, position.Y) * 16f + Vector2.One * 8f, new Color("#ffffff"), 1f);
                } else {
                    hasLast = true;
                }

                lastPosition = position;
            }
        }
    }

    private bool OnSegment(Vector2 a, Vector2 b, Vector2 c) {
        if (b.X <= Mathf.Max(a.X, c.X) && b.X >= Mathf.Min(a.X, c.X) &&
                b.Y <= Mathf.Max(a.Y, c.Y) && b.Y >= Mathf.Min(a.Y, c.Y))
            return true;

        return false;
    }

    private int PointOrientation(Vector2 a, Vector2 b, Vector2 c) {
        float val = (b.Y - a.Y) * (c.X - b.X) -
                    (b.X - a.X) * (c.Y - b.Y);

        if (val == 0) return 0;

        return (val > 0) ? 1 : 2;
    }

    private bool LinesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2) {
        int o1 = PointOrientation(a1, a2, b1);
        int o2 = PointOrientation(a1, a2, b2);
        int o3 = PointOrientation(b1, b2, a1);
        int o4 = PointOrientation(b1, b2, a2);


        if (o1 != o2 && o3 != o4)
            return true;

        if (o1 == 0 && OnSegment(a1, b1, a2)) return true;

        if (o2 == 0 && OnSegment(a1, b2, a2)) return true;

        if (o3 == 0 && OnSegment(b1, a1, b2)) return true;

        if (o4 == 0 && OnSegment(b1, a2, b2)) return true;

        return false;
    }
#endif
}