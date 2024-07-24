using Godot;
using Riptide;

public class PlayerNormal : State {
  private static float s_Speed = 80f;

  private Player _player;

  public PlayerNormal(string name, Player player) : base(name) {
    _player = player;
  }

  public override void Initialize() {
    _player.NetworkPoint.Register(nameof(DashRpc), DashRpc);
  }

  public override void PhsysicsUpdate(float delta) {
    if (_player.Knockback.Length() < 3.5f) {
      if (_player.Velocity.Length() > 3.5f) {
        _player.AnimationPlayer.Play("run");

        foreach (Equipment equipment in _player.EquippedEquipments.Values) {
          equipment.AnimationPlayer.Play("run");
        }
      } else {
        _player.AnimationPlayer.Play("idle");

        foreach (Equipment equipment in _player.EquippedEquipments.Values) {
          equipment.AnimationPlayer.Play("idle");
        }
      }
    }

    if (!_player.NetworkPoint.IsOwner) return;

    Vector2 movement = Vector2.Right * Input.GetAxis("move_left", "move_right") + Vector2.Up * Input.GetAxis("move_down", "move_up");

    float modifiedSpeed = s_Speed;
    foreach (Trinket trinket in _player.EquippedTrinkets) {
      modifiedSpeed = trinket.ModifySpeed(modifiedSpeed);
    }

    _player.Velocity = movement.Normalized() * modifiedSpeed + _player.Knockback;

    _player.MoveAndSlide();
  }

  public override void Exit() {
    _player.AnimationPlayer.Play("RESET");
    _player.AnimationPlayer.Advance(1f);
  }

  public override void OnInput(InputEvent inputEvent) {
    if (!_player.NetworkPoint.IsOwner) return;

    if (inputEvent.IsActionPressed("interact")) {
      Interactable interactable = Interactables.GetClosest(_player);

      if (interactable == null) return;

      interactable.Interact(_player);

      return;
    }

    if (!inputEvent.IsActionPressed("dash")) return;

    if (!GetState<PlayerDash>("dash").CanDash()) return;

    _player.NetworkPoint.BounceRpcToClients(nameof(DashRpc));

    GoToState("dash");
  }

  private void DashRpc(Message message) {
    if (_player.NetworkPoint.IsOwner) return;

    GoToState("dash");
  }
}
