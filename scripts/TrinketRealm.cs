using Godot;
using Networking;
using Riptide;
using System.Collections.Generic;

public partial class TrinketRealm : Node2D, NetworkPointUser {
	public static TrinketRealm Me;

	[Export] public PackedScene[] TrinketScenes = new PackedScene[] { };
	[Export] public PackedScene[] WeaponScenes = new PackedScene[] { };
	[Export] public ColorRect TrinketBackground;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private float _trinketBackgroundTargetAlpha = 0f;

	public override void _Ready() {
		Me = this;

		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(SpawnWeaponRpc), SpawnWeaponRpc);
		NetworkPoint.Register(nameof(SpawnWeaponClientRpc), SpawnWeaponClientRpc);
	}

	public override void _Process(double delta) {
		Color color = Me.TrinketBackground.Modulate;
		color.A = Mathf.Lerp(color.A, _trinketBackgroundTargetAlpha, 1f * (float)delta);

		Me.TrinketBackground.Modulate = color;
	}

	public static void EnterTrinketRealm(Altar altar) {
		Me._trinketBackgroundTargetAlpha = 1f;

		Me.NetworkPoint.SendRpcToServer(nameof(SpawnWeaponRpc), message => {
			message.AddInt(NetworkManager.LocalClient.Id);
			message.AddFloat(altar.GlobalPosition.X);
			message.AddFloat(altar.GlobalPosition.Y);
		});
	}

	public static void LeaveTinketRealm() {
		Me._trinketBackgroundTargetAlpha = 0f;
	}

	private void SpawnWeaponRpc(Message message) {
		GD.Print("Spawn Weapon");

		Me.NetworkPoint.SendRpcToClients(nameof(SpawnWeaponClientRpc), newMessage => {
			newMessage.AddString(Me.WeaponScenes[Game.RandomNumberGenerator.RandiRange(0, Me.WeaponScenes.Length - 1)].ResourcePath);
			newMessage.AddInt(message.GetInt());
			newMessage.AddFloat(message.GetFloat());
			newMessage.AddFloat(message.GetFloat());
		});
	}

	private void SpawnWeaponClientRpc(Message message) {
		GD.Print("Spawn Weapon client");

		string weaponPath = message.GetString();
		PackedScene weaponScene = ResourceLoader.Load<PackedScene>(weaponPath);

		Weapon weapon = NetworkManager.SpawnNetworkSafe<Weapon>(weaponScene, "Weapon");

		AddChild(weapon);

		if (message.GetInt() != NetworkManager.LocalClient.Id) return;

		Vector2 spawnPosition = new Vector2(message.GetFloat(), message.GetFloat());

		weapon.GlobalPosition = spawnPosition;
	}
}
