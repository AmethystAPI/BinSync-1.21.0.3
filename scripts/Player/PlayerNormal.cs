using Godot;
using Networking;
using Riptide;

public partial class PlayerNormal : NodeState, NetworkPointUser {
  [Export] public float Speed = 100f;

  public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

  private Player _player;
  private AnimationPlayer _animationPlayer;

  public override void _Ready() {
    NetworkPoint.Setup(this);

    NetworkPoint.Register(nameof(DashRpc), DashRpc);

    _player = GetParent().GetParent<Player>();

    _animationPlayer = _player.GetNode<AnimationPlayer>("AnimationPlayer");
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

    float modifiedSpeed = Speed;
    foreach (Trinket trinket in _player.EquippedTrinkets) {
      modifiedSpeed = trinket.ModifySpeed(modifiedSpeed);
    }

    _player.Velocity = movement.Normalized() * modifiedSpeed + _player.Knockback;

    _player.MoveAndSlide();
  }

  public override void Exit() {
    _animationPlayer.Play("RESET");
    _animationPlayer.Advance(1f);
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

    if (!GetState<PlayerDash>("Dash").CanDash()) return;

    NetworkPoint.BounceRpcToClients(nameof(DashRpc));

    GoToState("Dash");
  }

  private void DashRpc(Message message) {
    if (NetworkPoint.IsOwner) return;

    GoToState("Dash");
  }
}
