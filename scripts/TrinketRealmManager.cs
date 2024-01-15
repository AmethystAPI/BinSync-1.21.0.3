using Godot;
using Networking;
using Riptide;
using System.Collections.Generic;

public partial class TrinketRealmManager : Node2D, NetworkPointUser {
	private static TrinketRealmManager s_Me;

	[Export] public PackedScene[] TrinketScenes = new PackedScene[] { };

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();


	public override void _Ready() {
		s_Me = this;

		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(EnterTrinketRealmRpc), EnterTrinketRealmRpc);
	}

	public static void EnterTrinketRealm() {
		if (!NetworkManager.IsHost) return;

		List<int> clientIds = new List<int>();

		foreach (Connection connection in NetworkManager.LocalServer.Clients) {
			clientIds.Add(connection.Id);
		}

		foreach (int clientId in clientIds) {
			s_Me.NetworkPoint.SendRpcToClients(nameof(EnterTrinketRealmRpc), message => {
				message.AddInt(clientId);
				message.AddString(s_Me.TrinketScenes[new RandomNumberGenerator().RandiRange(0, s_Me.TrinketScenes.Length - 1)].ResourcePath);
			});
		}
	}

	public static void LeaveTinketRealm() {

	}

	private void EnterTrinketRealmRpc(Message message) {
		int clientId = message.GetInt();
		string lootScenePath = message.GetString();

		PackedScene lootScene = ResourceLoader.Load<PackedScene>(lootScenePath);

		Trinket trinket = NetworkManager.SpawnNetworkSafe<Trinket>(lootScene, "Loot");

		AddChild(trinket);

		if (NetworkManager.LocalClient.Id != clientId) return;

		GameUI.ShowTrinketBackground();

		Camera.EnableTrinketOffset();

		Player.LocalPlayer.EnterTrinketRealm(trinket);

		trinket.AnimateToPlayer(Player.LocalPlayer);
	}
}
