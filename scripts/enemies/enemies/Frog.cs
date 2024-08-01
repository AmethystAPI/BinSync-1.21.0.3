using Godot;
using Networking;
using Riptide;

public partial class Frog : Enemy {
	[Export] public PackedScene ProjectileScene;
	[Export] public PackedScene LargeProjectileScene;
	[Export] public Node2D ProjectileOrigin;

	private RandomNumberGenerator _random = new();

	public override void _Ready() {
		base._Ready();

		NetworkPoint.Register(nameof(SetRandomSeedRpc), SetRandomSeedRpc);
	}

	public override void AddStates() {
		base.AddStates();

		_stateMachine.Add(new Idle("idle", this) {
			Interval = new Vector2(2, 3)
		});

		_stateMachine.Add(new JumpAttack("attack", this) {
			Duration = 1f,
			OnJump = direciton => {
				SquashAndStretch.Trigger(new Vector2(1.4f, 0.6f), 8f);
			},
			OnLand = direction => {

				SquashAndStretch.Trigger(new Vector2(0.6f, 1.4f), 8f);

				for (int index = 0; index < 7; index++) {
					Vector2 offsetDirection = direction.Rotated(_random.RandfRange(-Mathf.Pi / 4f, Mathf.Pi / 4f));

					Projectile _projectile = (index < 2 ? LargeProjectileScene : ProjectileScene).Instantiate<Projectile>();

					_projectile.Source = this;
					_projectile.Speed *= _random.RandfRange(0.5f, 1.0f);

					GetParent().AddChild(_projectile);

					_projectile.GlobalPosition = GlobalPosition;
					_projectile.Position += offsetDirection * 2f;

					_projectile.LookAt(_projectile.GlobalPosition + offsetDirection);
				}
			}
		});

		if (NetworkManager.IsHost) NetworkPoint.SendRpcToClients(nameof(SetRandomSeedRpc), message => message.AddULong(_random.Seed));
	}

	public override bool CanDamage(Projectile projectile) {
		if (!base.CanDamage(projectile)) return false;

		if (_stateMachine.CurrentState == "attack") return false;

		return true;
	}

	public override void SyncPosition(float delta) {
		if (NetworkPoint.IsOwner) {
			_networkedPosition.Value = GlobalPosition;
		} else if (_stateMachine.CurrentState != "attack" && _networkedPosition.Synced) {
			if (_networkedPosition.Value.DistanceSquaredTo(GlobalPosition) > 64) GlobalPosition = _networkedPosition.Value;

			GlobalPosition = GlobalPosition.Lerp(_networkedPosition.Value, delta * 20.0f);
		}
	}

	private void SetRandomSeedRpc(Message message) {
		_random.Seed = message.GetULong();
	}
}
