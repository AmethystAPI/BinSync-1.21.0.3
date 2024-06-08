using Godot;

public partial class HighlightInteractable : Node2D {
    [Export] public Node2D Interactable;
    [Export] public Material DefaultMaterial;
    [Export] public Material HighlightMaterial;

    private Sprite2D _sprite;

    public override void _Ready() {
        _sprite = GetParent<Sprite2D>();
    }

    public override void _Process(double delta) {
        _sprite.Material = Interactables.IsActive(Interactable as Interactable) ? HighlightMaterial : DefaultMaterial;
    }
}