using Godot;
using Riptide;
using System;

public partial class Slime : CharacterBody2D, Damageable, Networking.NetworkNode
{
	[Export] public PackedScene ProjectileScene;
	[Export] public float Health = 3f;
	[Export] public float Speed = 10f;

	private Networking.RpcMap _rpcMap = new Networking.RpcMap();
	public Networking.RpcMap RpcMap => _rpcMap;

	private Networking.SyncedVariable<Vector2> _syncedPosition = new Networking.SyncedVariable<Vector2>(nameof(_syncedPosition), Vector2.Zero, Networking.Authority.Client, false, 50);

	private float _attackTimer = 2f;
	private Vector2 _knockback;

	public override void _Ready()
	{
		_rpcMap.Register(nameof(DamageRpc), DamageRpc);
		_rpcMap.Register(nameof(AttackRpc), AttackRpc);
		_rpcMap.Register(_syncedPosition, this);

		_syncedPosition.Value = GlobalPosition;

		GetParent<Room>().AddEnemy();
	}

	public override void _Process(double delta)
	{
		_syncedPosition.Sync();

		if (Game.IsOwner(this))
		{
			_syncedPosition.Value = GlobalPosition;
		}
		else
		{
			if (_syncedPosition.Value.DistanceSquaredTo(GlobalPosition) > 64) GlobalPosition = _syncedPosition.Value;

			GlobalPosition = GlobalPosition.Lerp(_syncedPosition.Value, (float)delta * 20.0f);
		}

		if (!Game.IsHost()) return;

		_attackTimer -= (float)delta;

		if (_attackTimer <= 0)
		{
			_attackTimer = 1f;

			Game.SendRpcToClients(this, nameof(AttackRpc), MessageSendMode.Reliable, message => { });
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		_knockback = _knockback.Lerp(Vector2.Zero, (float)delta * 12f);

		if (!Game.IsOwner(this)) return;

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
		if (!Game.IsOwner(projectile)) return;

		Game.SendRpcToAllClients(this, nameof(DamageRpc), MessageSendMode.Reliable, message =>
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
