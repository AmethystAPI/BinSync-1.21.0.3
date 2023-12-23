using Godot;
using System;

public partial class Slime : CharacterBody2D, Damageable
{
	[Export] public PackedScene ProjectileScene;

	private int _health = 3;
	private float _attackTimer = 2f;

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
		if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;

		Vector2 target = Player.Players[0].GlobalPosition;

		foreach (Player player in Player.Players)
		{
			if (GlobalPosition.DistanceTo(player.GlobalPosition) >= GlobalPosition.DistanceTo(target)) continue;

			target = player.GlobalPosition;
		}

		Velocity = (target - GlobalPosition).Normalized() * 10f;

		MoveAndSlide();
	}

	public void Damage(Projectile projectile)
	{
		if (projectile.GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;

		Rpc(nameof(DamageRpc), projectile.GetMultiplayerAuthority());
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void DamageRpc(int newAuthority)
	{
		SetMultiplayerAuthority(newAuthority);

		_health--;

		if (_health <= 0) QueueFree();
	}

	[Rpc(CallLocal = true)]
	public void AttackRpc()
	{
		Projectile projectile = ProjectileScene.Instantiate<Projectile>();

		projectile.Source = this;
		projectile.GlobalPosition = GlobalPosition;

		GetParent().AddChild(projectile);
	}

	public bool CanDamage(Projectile projectile)
	{
		return projectile.Source is Player;
	}

}
