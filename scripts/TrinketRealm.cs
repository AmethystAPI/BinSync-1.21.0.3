using Godot;
using Networking;
using Riptide;
using System.Collections.Generic;
using System.Net;

public partial class TrinketRealm : Node2D, NetworkPointUser {
	public static TrinketRealm Me;

	[Export] public PackedScene[] TrinketScenes = new PackedScene[] { };
	[Export] public PackedScene[] WeaponScenes = new PackedScene[] { };
	[Export] public ColorRect TrinketBackground;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private float _trinketBackgroundTargetAlpha = 0f;
	private List<Item> _spawnedItems = new List<Item>();

	public override void _Ready() {
		Me = this;

		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(SpawnItemsRpc), SpawnItemsRpc);
		NetworkPoint.Register(nameof(SpawnWeaponClientRpc), SpawnWeaponClientRpc);
		NetworkPoint.Register(nameof(SpawnTrinketRpc), SpawnTrinketRpc);
		NetworkPoint.Register(nameof(DespawnItemRpc), DespawnItemRpc);
	}

	public override void _Process(double delta) {
		Color color = Me.TrinketBackground.Modulate;
		color.A = Mathf.Lerp(color.A, _trinketBackgroundTargetAlpha, 1f * (float)delta);

		Me.TrinketBackground.Modulate = color;
	}

	public static void EnterTrinketRealm(Altar altar) {
		Me._trinketBackgroundTargetAlpha = 1f;

		Me.NetworkPoint.SendRpcToServer(nameof(SpawnItemsRpc), message => {
			message.AddInt(NetworkManager.LocalClient.Id);
			message.AddFloat(altar.GlobalPosition.X);
			message.AddFloat(altar.GlobalPosition.Y);
		});
	}

	public static void LeaveTinketRealm() {
		Me._trinketBackgroundTargetAlpha = 0f;
	}

	private void SpawnItemsRpc(Message message) {
		int targetPlayerId = message.GetInt();
		Vector2 altarPosition = new Vector2(message.GetFloat(), message.GetFloat());

		Me.NetworkPoint.SendRpcToClients(nameof(SpawnWeaponClientRpc), newMessage => {
			newMessage.AddString(Me.WeaponScenes[Game.RandomNumberGenerator.RandiRange(0, Me.WeaponScenes.Length - 1)].ResourcePath);
			newMessage.AddInt(targetPlayerId);
			newMessage.AddFloat(altarPosition.X);
			newMessage.AddFloat(altarPosition.Y);
		});

		for (int i = -1; i <= 1; i++) {
			Me.NetworkPoint.SendRpcToClients(nameof(SpawnTrinketRpc), newMessage => {
				newMessage.AddString(Me.TrinketScenes[Game.RandomNumberGenerator.RandiRange(0, Me.TrinketScenes.Length - 1)].ResourcePath);
				newMessage.AddInt(targetPlayerId);
				newMessage.AddFloat(altarPosition.X + i * 32f);
				newMessage.AddFloat(altarPosition.Y - 32f);
			});
		}
	}

	private void SpawnWeaponClientRpc(Message message) {
		string weaponPath = message.GetString();
		PackedScene weaponScene = ResourceLoader.Load<PackedScene>(weaponPath);

		Weapon weapon = NetworkManager.SpawnNetworkSafe<Weapon>(weaponScene, "Weapon");

		AddChild(weapon);

		if (message.GetInt() != NetworkManager.LocalClient.Id) {
			weapon.GlobalPosition = Player.LocalPlayer.GlobalPosition + Vector2.Down * 1000f;

			return;
		};

		Vector2 spawnPosition = new Vector2(message.GetFloat(), message.GetFloat());

		weapon.GlobalPosition = spawnPosition;

		_spawnedItems.Add(weapon);
	}

	private void SpawnTrinketRpc(Message message) {
		string trinketPath = message.GetString();
		PackedScene trinketScene = ResourceLoader.Load<PackedScene>(trinketPath);

		Trinket trinket = NetworkManager.SpawnNetworkSafe<Trinket>(trinketScene, "Trinket");

		AddChild(trinket);

		if (message.GetInt() != NetworkManager.LocalClient.Id) {
			trinket.GlobalPosition = Player.LocalPlayer.GlobalPosition + Vector2.Down * 1000f;

			return;
		};

		Vector2 spawnPosition = new Vector2(message.GetFloat(), message.GetFloat());

		trinket.GlobalPosition = spawnPosition;

		_spawnedItems.Add(trinket);

		trinket.Equipped += () => {
			foreach (Item item in _spawnedItems) {
				if (item.GetParent() != this) continue;

				if (item == trinket) continue;

				NetworkPoint.BounceRpcToClients(nameof(DespawnItemRpc), message => message.AddString(GetPathTo(item)));
			}

			LeaveTinketRealm();

			Player.LocalPlayer.LeaveTrinketRealm();
		};
	}

	private void DespawnItemRpc(Message message) {
		string path = message.GetString();

		GetNode(path).QueueFree();
	}
}
