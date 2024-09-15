using Godot;
using Networking;

public partial class LogSpirit : Enemy {
	[Export] public PackedScene ProjectileScene;
	[Export] public Node2D ProjectileOrigin;

	public override void AddStates() {
		base.AddStates();

		_stateMachine.Add(new RandomIdle("idle", this) {
			AttackStates = new string[] { "jump", "attack" },
			AttackWeights = new float[] { 1f, 1f },
		});

		_stateMachine.Add(new JumpAttack("jump", this) {
			OnJump = direction => {
				SquashAndStretch.Trigger(new Vector2(0.6f, 1.4f), 4f);
			},
			OnLand = direction => {
				SquashAndStretch.Trigger(new Vector2(1.4f, 0.6f), 10f);

				Camera.Shake(0.2f);
			}
		});

		_stateMachine.Add(new TimedAttack("attack", this) {
			OnPrepare = shootQueue => {
				shootQueue.Add(0.3f);

				SquashAndStretch.Trigger(new Vector2(1.4f, 0.6f), 8f);
			},
			OnShoot = direction => {
				SquashAndStretch.Trigger(new Vector2(0.6f, 1.4f), 8f);

				for (int index = 0; index < 3; index++) {
					Projectile _projectile = ProjectileScene.Instantiate<Projectile>();

					_projectile.Source = this;

					GetParent().AddChild(_projectile);

					_projectile.GlobalPosition = GlobalPosition;
					_projectile.Position += direction * 5f;

					_projectile.LookAt(_projectile.GlobalPosition + direction);
					_projectile.Rotate(Mathf.DegToRad(30f * (index - 1)));
				}
			}
		});
	}

	public override bool CanDamage(Projectile projectile) {
		if (!base.CanDamage(projectile)) return false;

		if (_stateMachine.CurrentState == "jump") return false;

		return true;
	}

	public override void SyncPosition(float delta) {
		if (NetworkPoint.IsOwner) {
			_networkedPosition.Value = GlobalPosition;
		} else if (_stateMachine.CurrentState != "jump" && _networkedPosition.Synced) {
			if (_networkedPosition.Value.DistanceSquaredTo(GlobalPosition) > 64) GlobalPosition = _networkedPosition.Value;

			GlobalPosition = GlobalPosition.Lerp(_networkedPosition.Value, delta * 20.0f);
		}
	}
}
