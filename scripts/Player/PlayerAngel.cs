public partial class PlayerAngel : State
{
  // if (Health <= 0)
  // {
  // 	_angelSwapTimer -= (float)delta;

  // 	if (_angelSwapTimer <= 0)
  // 	{
  // 		_angelSwapTimer = _randomNumberGenerator.RandfRange(0.8f, 1.2f);

  // 		if (_randomNumberGenerator.RandfRange(0f, 1f) < 0.5f) _angelTurn = -_angelTurn;
  // 	}

  // 	_angelAngle += Mathf.Pi * (float)delta * _angelTurn;

  // 	Velocity = Vector2.Right.Rotated(_angelAngle) * 50f;
  // }

  // foreach (Node2D body in _ressurectArea.GetOverlappingBodies())
  // 	{
  // 		if (!(body is Player)) continue;

  // 		if (body == this) continue;

  // 		Player player = (Player)body;

  // 		if (player.Health > 0) continue;

  // 		// Game.BounceRpcToClients(body, nameof(ReviveRpc), MessageSendMode.Reliable, message => { });
  // 	}
}
