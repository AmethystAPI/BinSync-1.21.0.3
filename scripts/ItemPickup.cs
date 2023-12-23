using Godot;
using System;

public partial class ItemPickup : Area2D
{
	[Export] public PackedScene Item;

	public override void _Input(InputEvent @event)
	{
		if (!@event.IsActionPressed("equip")) return;

		foreach (Node2D body in GetOverlappingBodies())
		{
			if (!(body is Player)) continue;

			if (body.GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) continue;

			(body as Player).EquipWeapon(Item);

			break;
		}
	}
}
