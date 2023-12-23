using Godot;
using System;

public partial class Slime : CharacterBody2D, Entity
{
	private int _health = 3;

	public void Damage(Projectile projectile)
	{
		Rpc(nameof(DamageRpc), projectile.GetMultiplayerAuthority());
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void DamageRpc(int newAuthority)
	{
		SetMultiplayerAuthority(newAuthority);

		_health--;

		if (_health <= 0) QueueFree();
	}
}
