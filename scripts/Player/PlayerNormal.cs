using Godot;

public partial class PlayerNormal : State {
  [Export] public float Speed = 100f;

  private Player _player;

  public override void _Ready() {
    _player = GetParent().GetParent<Player>();
  }

  public override void PhsysicsUpdate(float delta) {
    if (_player.Knockback.Length() < 3.5f) {
      if (_player.Velocity.Length() > 3.5f) {
        _player.AnimationPlayer.Play("run");
      } else {
        _player.AnimationPlayer.Play("idle");
      }
    }

    if (!_player.NetworkPoint.IsOwner) return;

    Vector2 movement = Vector2.Right * Input.GetAxis("move_left", "move_right") + Vector2.Up * Input.GetAxis("move_down", "move_up");

    float modifiedSpeed = Speed;
    foreach (Trinket trinket in _player.EquippedTrinkets) {
      modifiedSpeed = trinket.ModifySpeed(modifiedSpeed);
    }

    _player.Velocity = movement.Normalized() * modifiedSpeed + _player.Knockback;

    _player.MoveAndSlide();
  }

  public override void OnInput(InputEvent inputEvent) {
    if (!_player.NetworkPoint.IsOwner) return;

    if (!inputEvent.IsActionPressed("dash")) return;

    GoToState("Dash");
  }
}
