using Godot;

public partial class PlayerTrinket : State {
    [Export] public AnimationPlayer AnimationPlayer;

    public override void Enter() {
        AnimationPlayer.Play("awe");
    }
}
