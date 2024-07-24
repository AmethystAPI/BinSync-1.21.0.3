using Godot;

public partial class Slime : Enemy {
	[Export] public PackedScene ProjectileScene;
	[Export] public Node2D ProjectileOrigin;
	[Export] public Node2D JumpTransform;

	public override void AddStates() {
		base.AddStates();

		_stateMachine.Add(new Idle("idle", this));
		_stateMachine.Add(new Jump("attack", this) {
			Land = () => {
				Projectile projectile = ProjectileScene.Instantiate<Projectile>();

				projectile.Source = this;

				GetParent().AddChild(projectile);

				projectile.GlobalPosition = ProjectileOrigin.GlobalPosition;
			}
		});
	}

	public override bool CanDamage(Projectile projectile) {
		if (!base.CanDamage(projectile)) return false;

		if (_stateMachine.CurrentState == "attack") return false;

		return true;
	}

	public override void SyncPosition(float delta) {
		if (NetworkPoint.IsOwner) {
			_networkedPosition.Value = GlobalPosition;
		} else if (_stateMachine.CurrentState != "attack") {
			if (_networkedPosition.Value.DistanceSquaredTo(GlobalPosition) > 64) GlobalPosition = _networkedPosition.Value;

			GlobalPosition = GlobalPosition.Lerp(_networkedPosition.Value, delta * 20.0f);
		}
	}
}
