using Godot;

public partial class Equipment : Item {
  [Export] public string Slot = "Head";

  public AnimationPlayer AnimationPlayer;

  public override void _Ready() {
    AnimationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
  }
}