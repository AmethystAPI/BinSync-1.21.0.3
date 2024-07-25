using Godot;
using Networking;

public partial class CactusSpirit : Enemy {
	[Export] public PackedScene ProjectileScene;
	[Export] public Node2D ProjectileOrigin;

	public override void AddStates() {
		base.AddStates();

		_stateMachine.Add(new RangedApproach("idle", this));
		_stateMachine.Add(new TimedAttack("attack", this) {
			OnPrepare = shootQueue => {
				shootQueue.Add(0.3f);
				shootQueue.Add(0.3f + 0.12f);
				shootQueue.Add(0.3f + 0.12f * 2f);

				SquashAndStretch.Trigger(new Vector2(1.4f, 0.6f), 8f);
			},
			OnShoot = direction => {
				SquashAndStretch.Trigger(new Vector2(0.6f, 1.4f), 8f);

				Projectile _projectile = ProjectileScene.Instantiate<Projectile>();

				_projectile.Source = this;

				GetParent().AddChild(_projectile);

				_projectile.GlobalPosition = GlobalPosition;
				_projectile.Position += direction * 5f;

				_projectile.LookAt(_projectile.GlobalPosition + direction);

				AnimationPlayer.Play("attack");
			}
		});
	}
}
