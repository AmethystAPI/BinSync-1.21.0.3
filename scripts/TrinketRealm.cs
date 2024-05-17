using Godot;
using Networking;
using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;

public partial class TrinketRealm : Node2D, NetworkPointUser {
	public static TrinketRealm Me;

	[Export] public PackedScene[] TrinketScenes = new PackedScene[0];
	[Export] public PackedScene[] WeaponScenes = new PackedScene[0];
	[Export] public ColorRect TrinketBackground;
	[Export] public PackedScene[] EnemyScenes = new PackedScene[0];

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private float _trinketBackgroundTargetAlpha = 0f;
	private List<Item> _spawnedItems = new List<Item>();
	private List<PackedScene> _enemiesToSpawn = new List<PackedScene>();
	private int _playersToComplete = 0;

	public override void _Ready() {
		Me = this;

		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(PlayerEnterRpc), PlayerEnterRpc);
		NetworkPoint.Register(nameof(PlayerLeaveRpc), PlayerLeaveRpc);
		NetworkPoint.Register(nameof(SpawnEnemyRpc), SpawnEnemyRpc);
		NetworkPoint.Register(nameof(SpawnItemsRpc), SpawnItemsRpc);
		NetworkPoint.Register(nameof(SpawnWeaponClientRpc), SpawnWeaponClientRpc);
		NetworkPoint.Register(nameof(SpawnTrinketRpc), SpawnTrinketRpc);
		NetworkPoint.Register(nameof(DespawnItemRpc), DespawnItemRpc);
		NetworkPoint.Register(nameof(AddEnemyToSpawnRpc), AddEnemyToSpawnRpc);
	}

	public override void _Process(double delta) {
		Color color = Me.TrinketBackground.Modulate;
		color.A = Mathf.Lerp(color.A, _trinketBackgroundTargetAlpha, 1f * (float)delta);

		Me.TrinketBackground.Modulate = color;
	}

	public static void EnterTrinketRealm(Altar altar) {
		Me._spawnedItems.Clear();
		Me._enemiesToSpawn.Clear();

		Me.TrinketBackground.GlobalPosition = altar.GlobalPosition - Me.TrinketBackground.Size / 2f;

		Me._trinketBackgroundTargetAlpha = 1f;

		Me.NetworkPoint.SendRpcToServer(nameof(SpawnItemsRpc), message => {
			message.AddInt(NetworkManager.LocalClient.Id);
			message.AddFloat(altar.GlobalPosition.X);
			message.AddFloat(altar.GlobalPosition.Y);
		});

		Me.NetworkPoint.SendRpcToServer(nameof(PlayerEnterRpc));
	}

	public static void LeaveTinketRealm() {
		Me._trinketBackgroundTargetAlpha = 0f;

		Me.NetworkPoint.SendRpcToServer(nameof(PlayerLeaveRpc));
	}

	private void PlayerEnterRpc(Message message) {
		if (_playersToComplete != 0) return;

		_playersToComplete = NetworkManager.LocalServer.Clients.Length;
	}

	private void PlayerLeaveRpc(Message message) {
		_playersToComplete--;

		if (_playersToComplete != 0) return;

		foreach (PackedScene enemyScene in _enemiesToSpawn) {
			NetworkPoint.SendRpcToClients(nameof(SpawnEnemyRpc), message => {
				message.AddString(enemyScene.ResourcePath);

				message.AddFloat(new RandomNumberGenerator().RandfRange(-48f, 48f));
				message.AddFloat(new RandomNumberGenerator().RandfRange(-48f, 48f));
			});
		}
	}

	private void SpawnEnemyRpc(Message message) {
		string path = message.GetString();
		PackedScene enemyScene = ResourceLoader.Load<PackedScene>(path);

		Enemy enemy = NetworkManager.SpawnNetworkSafe<Enemy>(enemyScene, "Enemy");

		Game.CurrentRoom.AddChild(enemy);
		enemy.GlobalPosition = Game.CurrentRoom.GlobalPosition + new Vector2(message.GetFloat(), message.GetFloat());

		if (NetworkManager.IsHost) enemy.Activate();
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

		RandomNumberGenerator random = new RandomNumberGenerator();
		PackedScene enemy = EnemyScenes[random.RandiRange(0, EnemyScenes.Length - 1)];
		int amount = random.RandiRange(4, 10);

		Label label = new Label {
			Text = enemy.ResourcePath.Split("/").Last() + " " + amount.ToString() + "x",
			Scale = new Vector2(0.2f, 0.2f)
		};
		trinket.AddChild(label);

		trinket.Equipped += () => {
			foreach (Item item in _spawnedItems) {
				if (item.GetParent() != this) continue;

				if (item == trinket) continue;

				NetworkPoint.BounceRpcToClients(nameof(DespawnItemRpc), message => message.AddString(GetPathTo(item)));
			}

			for (int i = 0; i < amount; i++) {
				NetworkPoint.SendRpcToServer(nameof(AddEnemyToSpawnRpc), message => message.AddString(enemy.ResourcePath));
			}

			LeaveTinketRealm();

			Player.LocalPlayer.LeaveTrinketRealm();

			label.QueueFree();
		};
	}

	private void AddEnemyToSpawnRpc(Message message) {
		_enemiesToSpawn.Add(ResourceLoader.Load<PackedScene>(message.GetString()));
	}

	private void DespawnItemRpc(Message message) {
		string path = message.GetString();

		GetNode(path).QueueFree();
	}
}
