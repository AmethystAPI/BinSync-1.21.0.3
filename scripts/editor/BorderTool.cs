using System.Collections.Generic;
using System.Linq;
using Godot;

[Tool]
public partial class BorderTool : Node2D {
    [Export] public Texture2D BorderDecoration;
    [Export] public TileMap TileMap;

    private bool _justPressedGenerate = false;
    private TileMapLayer _screen;
    private Node2D _paralaxOrigin1;
    private Node2D _paralaxOrigin2;
    private HashSet<Vector2I> _edgeTiles = new HashSet<Vector2I>();
    private HashSet<Vector2I> _finalEdgeTiles = new HashSet<Vector2I>();
    private List<Vector2> _splinePoints = new List<Vector2>();

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

        foreach (Node child in _paralaxOrigin2.GetChildren()) {
            if (child.Name == "Screen") continue;

            child.QueueFree();
        }

        DetectEdges();

        QueueRedraw();
    }

    private void DetectEdges() {
        _edgeTiles.Clear();
        _finalEdgeTiles.Clear();
        _splinePoints.Clear();

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
                    _splinePoints.Add(new Vector2(position.X, position.Y) * 16f);
                }
            }
        }

        for (int index = 0; index < _splinePoints.Count; index++) {
            _splinePoints[index] += _splinePoints[index] * 0.126f;
        }

        foreach (Vector2 position in _splinePoints) {
            Sprite2D sprite = new Sprite2D() {
                Texture = BorderDecoration
            };

            _paralaxOrigin2.AddChild(sprite);

            sprite.Position = position + Vector2.One * 8f;
            sprite.Scale = Vector2.One * 0.75f;
            sprite.Owner = GetOwner();
        }
    }

    public override void _Draw() {
        foreach (Vector2I position in _finalEdgeTiles) {
            DrawCircle(new Vector2(position.X, position.Y) * 16f + Vector2.One * 8f, 4f, new Color("#00ff00"), true);
        }

        foreach (Vector2 position in _splinePoints) {
            DrawCircle(position + Vector2.One * 8f, 4f, new Color("#0000ff"), true);
        }
    }
#endif
}