using Godot;

public partial class PlayerDash : State
{
  [Export] public float Duration = 0.06f;
  [Export] public float Speed = 700f;

  private Player _player;
  private Vector2 _dashDirection;
  private float _dashTimer = 0;

  public override void _Ready()
  {
    _player = GetParent().GetParent<Player>();
  }

  public override void Enter()
  {
    if (!Game.IsOwner(this)) return;

    _dashTimer = Duration;

    _dashDirection = (_player.GetGlobalMousePosition() - _player.GlobalPosition).Normalized();
  }

  public override void PhsysicsUpdate(float delta)
  {
    if (!Game.IsOwner(this)) return;

    _player.Velocity = _dashDirection * Speed;

    _dashTimer -= delta;

    if (_dashTimer <= 0) GoToState("Normal");

    _player.MoveAndSlide();
  }
}
