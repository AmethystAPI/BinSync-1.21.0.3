using Godot;
using Networking;
using Riptide;

public partial class SwampFlower : Enemy {
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

		int firedProjectiles = 0;

		_stateMachine.Add(new TimedAttack("attack", this) {
			Duration = 1f,
			OnPrepare = shootQueue => {
				for (int index = 0; index < 7; index++) {
					shootQueue.Add(0.5f + _random.RandfRange(-0.1f, 0.1f));
				}

				SquashAndStretch.Trigger(new Vector2(1.4f, 0.6f), 8f);

				firedProjectiles = 0;
			},
			OnShoot = direction => {
				direction = direction.Rotated(_random.RandfRange(0f, Mathf.Pi * 2f));

				SquashAndStretch.Trigger(new Vector2(0.6f, 1.4f), 8f);

				Projectile _projectile = (firedProjectiles < 2 ? LargeProjectileScene : ProjectileScene).Instantiate<Projectile>();

				_projectile.Source = this;
				_projectile.Speed *= _random.RandfRange(0.5f, 1.0f);

				GetParent().AddChild(_projectile);

				_projectile.GlobalPosition = GlobalPosition;
				_projectile.Position += direction * 5f;

				_projectile.LookAt(_projectile.GlobalPosition + direction);


				AnimationPlayer.Play("attack");

				firedProjectiles++;
			}
		});

		if (NetworkManager.IsHost) NetworkPoint.SendRpcToClients(nameof(SetRandomSeedRpc), message => message.AddULong(_random.Seed));
	}

	protected override void DamageRpc(Message message) {
		base.DamageRpc(message);

		if (Dead) return;

		if (_stateMachine.CurrentState == "idle") _stateMachine.GoToState("attack");
	}

	private void SetRandomSeedRpc(Message message) {
		_random.Seed = message.GetULong();
	}
}
