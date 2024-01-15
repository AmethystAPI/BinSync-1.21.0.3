using Godot;

public partial class PlayerTrinket : State {
    [Export] public Sprite2D Sprite;
    [Export] public int HoldFrame;
    [Export] public float Duration = 4f;

    private float _timer;

    public override void Enter() {
        Sprite.Frame = HoldFrame;

        _timer = Duration;
    }

    public override void Update(float delta) {
        _timer -= delta;

        if (_timer <= 0) GoToState("Normal");
    }
}
