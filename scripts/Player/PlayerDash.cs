using Godot;

public class PlayerDash : State {
  [Export] public float Duration = 0.333f;
  [Export] public float Speed = 200f;
  [Export] public float Cooldown = 0.4f;

  private Player _player;
  private Area2D _ressurectArea;

  private Vector2 _dashDirection;
  private float _dashTimer = 0;
  private float _cooldownTimer;

  public PlayerDash(string name, Player player) : base(name) {
    _player = player;
  }

  public override void Initialize() {
    _ressurectArea = _player.GetNode<Area2D>("RessurectArea");
  }

  public override void Enter() {
    _player.AnimationPlayer.Play("dash");

    foreach (Equipment equipment in _player.EquippedEquipments.Values) {
      equipment.AnimationPlayer.Play("dash");
    }

    _dashTimer = Duration;

    if (!_player.NetworkPoint.IsOwner) return;

    _dashDirection = (_player.GetGlobalMousePosition() - _player.GlobalPosition).Normalized();
  }

  public override void UpdateBackground(float delta) {
    _cooldownTimer -= delta;
  }

  public override void PhsysicsUpdate(float delta) {

    _dashTimer -= delta;

    if (_dashTimer <= 0) GoToState("normal");

    if (!_player.NetworkPoint.IsOwner) return;

    _player.Velocity = _dashDirection * Speed;

    _player.MoveAndSlide();

    foreach (Node2D body in _ressurectArea.GetOverlappingBodies()) {
      if (!(body is Player)) continue;

      if (body == _player) continue;

      Player player = (Player)body;

      if (player.Health > 0) continue;

      player.Revive();

      break;
    }
  }

  public override void Exit() {
    _cooldownTimer = Cooldown;
  }

  public bool CanDash() {
    return _cooldownTimer <= 0;
  }
}
