using Godot;
using System;

public partial class Trinket : Item
{
	Player _equippingPlayer;

	public override void Equip(Player player)
	{
		base.Equip(player);

		_equippingPlayer = player;
	}

	public virtual float ModifySpeed(float speed)
	{
		return speed;
	}

	public virtual void ModifyProjectile(Weapon weapon, Projectile projectile)
	{

	}
}
