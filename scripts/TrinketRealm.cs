using Godot;
using Networking;
using Riptide;
using System.Collections.Generic;

public partial class TrinketRealm : Node2D, NetworkPointUser {
	private static TrinketRealm s_Me;

	[Export] public PackedScene[] TrinketScenes = new PackedScene[] { };
	[Export] public ColorRect TrinketBackground;
	[Export] public ColorRect Flash;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private Trinket _localNewTrinket;
	private float _trinketBackgroundTargetAlpha = 0f;
	private float _flashTargetAlpha = 0f;
	private float _flashSpeed;

	public override void _Ready() {
		s_Me = this;

		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(EnterTrinketRealmRpc), EnterTrinketRealmRpc);
	}

	public override void _Process(double delta) {
		Color color = s_Me.TrinketBackground.Modulate;
		color.A = Mathf.Lerp(color.A, _trinketBackgroundTargetAlpha, 1f * (float)delta);

		s_Me.TrinketBackground.Modulate = color;


		UpdateFlashColor((float)delta);
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

	private void EnterTrinketRealmRpc(Message message) {
		int clientId = message.GetInt();
		string lootScenePath = message.GetString();

		PackedScene lootScene = ResourceLoader.Load<PackedScene>(lootScenePath);

		Trinket trinket = NetworkManager.SpawnNetworkSafe<Trinket>(lootScene, "Loot");

		AddChild(trinket);

		if (NetworkManager.LocalClient.Id != clientId) return;

		_localNewTrinket = trinket;

		_trinketBackgroundTargetAlpha = 1f;

		Camera.EnterTrinketRealm();

		Player.LocalPlayer.EnterTrinketRealm();

		trinket.AnimateToPlayer(Player.LocalPlayer);

		GlobalPosition = Player.LocalPlayer.GlobalPosition;

		Delay.Execute(4f, s_Me.StartFlash);
	}

	private void StartFlash() {
		_flashTargetAlpha = 1f;
		_flashSpeed = 8f;

		Delay.Execute(0.5f, s_Me.LeaveTinketRealm);
	}

	private void LeaveTinketRealm() {
		_flashTargetAlpha = 0f;
		_flashSpeed = 10f;

		_trinketBackgroundTargetAlpha = 0f;
		TrinketBackground.Modulate = new Color(1, 1, 1, 0);

		Camera.LeaveTrinketRealm();

		Player.LocalPlayer.LeaveTrinketRealm();
		Player.LocalPlayer.Equip(_localNewTrinket);
	}

	private void UpdateFlashColor(float delta) {
		Color color = s_Me.Flash.Modulate;
		color.A = Mathf.Lerp(color.A, _flashTargetAlpha, s_Me._flashSpeed * delta);

		s_Me.Flash.Modulate = color;
	}
}
