using Godot;
using System;

public partial class Trinket : Item
{
	public virtual float ModifySpeed(float speed)
	{
		return speed;
	}
}
