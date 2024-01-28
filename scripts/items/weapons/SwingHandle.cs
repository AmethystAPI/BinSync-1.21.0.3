using Godot;

public partial class SwingHandle : Node2D {
    [Export] public float Speed = 24f;
    [Export] public float Range = 45f;

    private float _targetRotation;

    public override void _Ready() {
        _targetRotation = Range;
    }

    public override void _Process(double delta) {
        Node2D parent = GetParent<Node2D>().GetParent<Node2D>();

        float parentRotation = parent.Rotation % (Mathf.Pi * 2f);
        if (parentRotation < 0) parentRotation = Mathf.Pi * 2f + parentRotation;

        parent.Scale = new Vector2(1, 1);

        if (parentRotation > Mathf.Pi * 0.5f && parentRotation < Mathf.Pi * 1.5f) {
            parent.Scale = new Vector2(1, -1);
        }

        Rotation = Mathf.Lerp(Rotation, _targetRotation / 180f * Mathf.Pi, Speed * (float)delta);
    }

    public void Swing() {
        _targetRotation = -_targetRotation;
    }
}
