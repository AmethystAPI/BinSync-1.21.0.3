using Godot;

public class PlayerAngel : State {
  private Player _player;
  private float _angelAngle;
  private float _angelSwapTimer;
  private int _angelTurn = 1;
  private RandomNumberGenerator _randomNumberGenerator = new RandomNumberGenerator();

  public PlayerAngel(string name, Player player) : base(name) {
    _player = player;
  }

  public override void PhsysicsUpdate(float delta) {
    _player.AnimationPlayer.Play("dead");

    if (!_player.NetworkPoint.IsOwner) return;

    _angelSwapTimer -= delta;

    if (_angelSwapTimer <= 0) {
      _angelSwapTimer = _randomNumberGenerator.RandfRange(0.8f, 1.2f);

      if (_randomNumberGenerator.RandfRange(0f, 1f) < 0.5f) _angelTurn = -_angelTurn;
    }

    _angelAngle += Mathf.Pi * delta * _angelTurn;

    _player.Velocity = Vector2.Right.Rotated(_angelAngle) * 50f;

    _player.MoveAndSlide();
  }
}
