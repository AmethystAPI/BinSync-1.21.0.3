using Godot;
using Networking;
using System;

public partial class Temple : Room {
	[Export] public PackedScene[] Weapons = new PackedScene[0];

	private Node2D _itemSpawn;

	public override void _Ready() {
		base._Ready();

		_itemSpawn = GetNodeOrNull<Node2D>("ItemSpawn");
	}

	internal override void SpawnComponents() {
		PackedScene weaponScene = Weapons[Game.RandomNumberGenerator.RandiRange(0, Weapons.Length - 1)];

		NetworkManager.SpawnNetworkSafe<Weapon>(weaponScene, "Weapon");
	}
}
