using Godot;

public partial class SquashAndStretch : Node {
    [Export] public Node2D Target;

    private float _speed = 12f;

    public override void _Process(double delta) {
        Target.Scale = Target.Scale.Lerp(Vector2.One, _speed * (float)delta);
    }

    public void Trigger(Vector2 scale, float speed) {
        _speed = speed;

        Target.Scale = scale;
    }
}
