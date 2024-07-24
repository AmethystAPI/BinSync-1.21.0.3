using Godot;
using Networking;

public partial class CactusSpirit : Enemy {
	[Export] public PackedScene ProjectileScene;
	[Export] public Node2D ProjectileOrigin;

	public override void AddStates() {
		base.AddStates();

		_stateMachine.Add(new RangedApproach("idle", this));
		_stateMachine.Add(new JumpAttack("attack", this) {
			OnLand = () => {
				Projectile projectile = ProjectileScene.Instantiate<Projectile>();

				projectile.Source = this;

				GetParent().AddChild(projectile);

				projectile.GlobalPosition = ProjectileOrigin.GlobalPosition;
			}
		});
	}
}
