using Godot;

public partial class SquashAndStretch : Node {
    [Export] public Node2D Target;
    [Export] public Vector2 Scale = new Vector2(1.1f, 0.8f);
    [Export] public float Speed = 12f;

    public override void _Process(double delta) {
        Target.Scale = Target.Scale.Lerp(Vector2.One, Speed * (float)delta);
    }

    public void Activate() {
        Target.Scale = Scale;
    }
}
