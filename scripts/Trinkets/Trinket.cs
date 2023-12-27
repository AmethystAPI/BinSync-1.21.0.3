using Godot;
using System;

public partial class Trinket : Item
{
	public virtual float ModifySpeed(float speed)
	{
		return speed;
	}

	public virtual void ModifyProjectile(Weapon weapon, Projectile projectile)
	{

	}
}
