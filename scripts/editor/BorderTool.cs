using System.Collections.Generic;
using System.Linq;
using Godot;

[Tool]
public partial class BorderTool : Node {
    [Export] public Texture2D BorderDecoration;

    private float _timer = 0.5f;

#if TOOLS
    public override void _Process(double delta) {
        _timer -= (float)delta;

        if (_timer <= 0) {

            Generate();
            _timer = 0.5f;
        }
    }

    private void Generate() {
        RectangleShape2D shape = GetNode<CollisionShape2D>("Area").Shape as RectangleShape2D;

        Node2D paralaxOrigin1 = GetNode<Node2D>("Paralax/Origin");
        Node2D paralaxOrigin2 = GetNode<Node2D>("Paralax2/Origin");
    }
#endif
}