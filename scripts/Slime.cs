using Godot;
using Networking;
using Riptide;
using System;

public partial class Slime : CharacterBody2D, Damageable, NetworkPointUser
{
	[Export] public PackedScene ProjectileScene;
	[Export] public float Health = 3f;
	[Export] public float Speed = 10f;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private NetworkedVariable<Vector2> _networkedPosition = new NetworkedVariable<Vector2>(Vector2.Zero);

	private float _attackTimer = 2f;
	private Vector2 _knockback;

	public override void _Ready()
	{
		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(_networkedPosition), _networkedPosition);
		NetworkPoint.Register(nameof(DamageRpc), DamageRpc);
		NetworkPoint.Register(nameof(AttackRpc), AttackRpc);

		_networkedPosition.Value = GlobalPosition;

		GetParent<Room>().AddEnemy();
	}

	public override void _Process(double delta)
	{
		_networkedPosition.Sync();

		if (NetworkPoint.IsOwner)
		{
			_networkedPosition.Value = GlobalPosition;
		}
		else
		{
			if (_networkedPosition.Value.DistanceSquaredTo(GlobalPosition) > 64) GlobalPosition = _networkedPosition.Value;

			GlobalPosition = GlobalPosition.Lerp(_networkedPosition.Value, (float)delta * 20.0f);
		}

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
		_knockback = _knockback.Lerp(Vector2.Zero, (float)delta * 12f);

		if (!NetworkPoint.IsOwner) return;

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

	public void Damage(Projectile projectile)
	{
		if (!NetworkPoint.IsOwner) return;

		NetworkPoint.BounceRpcToClients(nameof(DamageRpc), message =>
		{
			message.AddInt(projectile.GetMultiplayerAuthority());

			Vector2 knockback = projectile.GlobalTransform.BasisXform(Vector2.Right) * 200f * projectile.Knockback;

			message.AddFloat(knockback.X);
			message.AddFloat(knockback.Y);

			message.AddFloat(projectile.Damage);
		});
	}

	public bool CanDamage(Projectile projectile)
	{
		return projectile.Source is Player;
	}

	private void DamageRpc(Message message)
	{
		SetMultiplayerAuthority(message.GetInt());

		_knockback = new Vector2(message.GetFloat(), message.GetFloat());

		Health -= message.GetFloat();

		if (Health <= 0)
		{
			GetParent<Room>().RemoveEnemy();

			QueueFree();
		}
	}

	private void AttackRpc(Message message)
	{
		Projectile projectile = ProjectileScene.Instantiate<Projectile>();

		projectile.Source = this;

		GetParent().AddChild(projectile);

		projectile.GlobalPosition = GlobalPosition;
	}
}
