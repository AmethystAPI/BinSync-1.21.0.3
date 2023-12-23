using Godot;
using System;

public partial class Slime : CharacterBody2D, Damageable
{
	[Export] public PackedScene ProjectileScene;

	private int _health = 3;
	private float _attackTimer = 2f;
	private Vector2 _knockback;

	public override void _Ready()
	{
		GetParent<Room>().AddEnemy();
	}

	public override void _Process(double delta)
	{
		if (!Game.Me.IsHost) return;

		_attackTimer -= (float)delta;

		if (_attackTimer <= 0)
		{
			_attackTimer = 1f;

			Rpc(nameof(AttackRpc));
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		_knockback = _knockback.Lerp(Vector2.Zero, (float)delta * 12f);

		if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;

		if (_knockback.LengthSquared() < 0.1f)
		{
			Vector2 target = Player.Players[0].GlobalPosition;

			foreach (Player player in Player.Players)
			{
				if (GlobalPosition.DistanceTo(player.GlobalPosition) >= GlobalPosition.DistanceTo(target)) continue;

				target = player.GlobalPosition;
			}

			Velocity = (target - GlobalPosition).Normalized() * 10f;
		}
		else
		{
			Velocity = _knockback;
		}

		MoveAndSlide();
	}

	public void Damage(Projectile projectile)
	{
		if (projectile.GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;

		Rpc(nameof(DamageRpc), projectile.GetMultiplayerAuthority(), projectile.GlobalTransform.BasisXform(Vector2.Right) * 400f);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void DamageRpc(int newAuthority, Vector2 knockback)
	{
		SetMultiplayerAuthority(newAuthority);

		_health--;

		_knockback = knockback;

		if (_health <= 0)
		{
			GetParent<Room>().RemoveEnemy();

			QueueFree();
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void AttackRpc()
	{
		Projectile projectile = ProjectileScene.Instantiate<Projectile>();

		projectile.Source = this;

		GetParent().AddChild(projectile);

		projectile.GlobalPosition = GlobalPosition;
	}

	public bool CanDamage(Projectile projectile)
	{
		return projectile.Source is Player;
	}

}
