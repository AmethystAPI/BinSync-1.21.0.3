using Godot;
using Networking;
using Riptide;

public partial class Slime : Enemy
{
	[Export] public PackedScene ProjectileScene;
	[Export] public float Speed = 10f;

	private float _attackTimer = 2f;

	public override void _Ready()
	{
		base._Ready();

		NetworkPoint.Register(nameof(AttackRpc), AttackRpc);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (!NetworkManager.IsHost) return;

		_attackTimer -= (float)delta;

		if (_attackTimer <= 0)
		{
			_attackTimer = 1f;

			NetworkPoint.SendRpcToClients(nameof(AttackRpc));
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!NetworkPoint.IsOwner) return;

		_knockback = _knockback.Lerp(Vector2.Zero, (float)delta * 12f);

		if (Player.AlivePlayers.Count == 0) return;

		if (_knockback.LengthSquared() < 0.1f)
		{
			Vector2 target = Player.AlivePlayers[0].GlobalPosition;

			foreach (Player player in Player.AlivePlayers)
			{
				if (GlobalPosition.DistanceTo(player.GlobalPosition) >= GlobalPosition.DistanceTo(target)) continue;

				target = player.GlobalPosition;
			}

			Velocity = (target - GlobalPosition).Normalized() * Speed;
		}
		else
		{
			Velocity = _knockback;
		}

		MoveAndSlide();
	}

	private void AttackRpc(Message message)
	{
		Projectile projectile = ProjectileScene.Instantiate<Projectile>();

		projectile.Source = this;

		GetParent().AddChild(projectile);

		projectile.GlobalPosition = GlobalPosition;
	}
}
