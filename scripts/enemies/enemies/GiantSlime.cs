using Godot;

public partial class GiantSlime : Enemy {
	[Export] public PackedScene[] Summons = new PackedScene[0];
	[Export] public Vector2I SummonAmmount = new Vector2I(3, 4);
	[Export] public PackedScene ProjectileScene;
	[Export] public Node2D ProjectileOrigin;

	public override void AddStates() {
		base.AddStates();

		_stateMachine.Add(new RandomIdle("idle", this) { AttackStates = new string[] { "jump", "summon" }, AttackWeights = new float[] { 1f, 0.4f } });

		_stateMachine.Add(new JumpAttack("jump", this) {
			Speed = 30f,
			Duration = 1.5f,
			Height = 32f,
			OnJump = () => {
				SquashAndStretch.Trigger(new Vector2(0.6f, 1.4f), 4f);
			},
			OnLand = () => {
				SquashAndStretch.Trigger(new Vector2(1.4f, 0.6f), 10f);

				Camera.Shake(2f);

				Projectile projectile = ProjectileScene.Instantiate<Projectile>();

				projectile.Source = this;

				GetParent().AddChild(projectile);

				projectile.GlobalPosition = ProjectileOrigin.GlobalPosition;
			}
		});

		_stateMachine.Add(new Summon("summon", this) {
			Summons = Summons,
			SummonAmmount = SummonAmmount,
			OnSummon = () => {
				SquashAndStretch.Trigger(new Vector2(0.6f, 1.4f), 8f);
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
		} else if (_stateMachine.CurrentState != "attack" && _networkedPosition.Synced) {
			if (_networkedPosition.Value.DistanceSquaredTo(GlobalPosition) > 64) GlobalPosition = _networkedPosition.Value;

			GlobalPosition = GlobalPosition.Lerp(_networkedPosition.Value, delta * 20.0f);
		}
	}
}
