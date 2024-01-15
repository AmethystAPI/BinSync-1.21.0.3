using Godot;

public partial class PlayerTrinket : State {
    [Export] public Sprite2D Sprite;
    [Export] public int HoldFrame;

    public override void Enter() {
        Sprite.Frame = HoldFrame;
    }
}
