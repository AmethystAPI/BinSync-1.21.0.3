using Godot;

public partial class PlayerDash : State {
  [Export] public float Duration = 0.06f;
  [Export] public float Speed = 700f;
  [Export] public float Cooldown = 0.2f;

  private Player _player;
  private Vector2 _dashDirection;
  private float _dashTimer = 0;
  private Area2D _ressurectArea;
  private float _cooldownTimer;

  public override void _Ready() {
    _player = GetParent().GetParent<Player>();

    _ressurectArea = _player.GetNode<Area2D>("RessurectArea");
  }

  public override void Enter() {
    _player.AnimationPlayer.Play("dash");

    if (!_player.NetworkPoint.IsOwner) return;

    _dashTimer = Duration;

    _dashDirection = (_player.GetGlobalMousePosition() - _player.GlobalPosition).Normalized();
  }

  public override void PhsysicsUpdate(float delta) {
    if (!_player.NetworkPoint.IsOwner) return;

    _player.Velocity = _dashDirection * Speed;

    _dashTimer -= delta;

    if (_dashTimer <= 0) GoToState("Normal");

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

  public override void _Process(double delta) {
    _cooldownTimer -= (float)delta;
  }

  public bool CanDash() {
    return _cooldownTimer <= 0;
  }
}
