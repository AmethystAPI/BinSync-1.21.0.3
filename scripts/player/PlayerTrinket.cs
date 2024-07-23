using Godot;

public partial class PlayerTrinket : NodeState {
    [Export] public AnimationPlayer AnimationPlayer;

    public override void Enter() {
        AnimationPlayer.Play("awe");
    }
}
