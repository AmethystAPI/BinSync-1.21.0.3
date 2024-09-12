using System;
using Godot;
using Networking;
using Steamworks;

public partial class MainMenu : Control {
	[Export] public Control StartControls;
	[Export] public Control LobbyControls;
	[Export] public Button HatEquipmentButton;
	[Export] public Button BodyEquipmentButton;

	[Export] public PackedScene[] HatEquipmentScenes;
	[Export] public PackedScene[] BodyEquipmentScenes;

	public override void _Ready() {
		Player.HatCosmetic = HatEquipmentScenes[0];
		Player.BodyCosmetic = BodyEquipmentScenes[0];

		NetworkManager.JoinedServer += () => {
			Visible = false;
		};
	}

	public void Host() {
		NetworkManager.Host();

		StartControls.Visible = false;
		LobbyControls.Visible = true;
	}

	public void Join() {
		SteamFriends.ActivateGameOverlay("friends");
	}

	public void Invite() {
		GD.Print(NetworkManager.CurrentLobby);
		SteamFriends.ActivateGameOverlayInviteDialog(NetworkManager.CurrentLobby);
	}

	public void Start() {
		if (!NetworkManager.IsHost) return;

		Game.Start();

		Visible = false;
	}

	public void NextHatEquipment() {
		int hatIndex = Array.IndexOf(HatEquipmentScenes, Player.HatCosmetic);
		hatIndex++;

		if (hatIndex >= HatEquipmentScenes.Length) hatIndex = 0;

		HatEquipmentButton.Text = HatEquipmentScenes[hatIndex].ResourcePath;
		Player.HatCosmetic = HatEquipmentScenes[hatIndex];
	}

	public void NextBodyEquipment() {
		int bodyIndex = Array.IndexOf(BodyEquipmentScenes, Player.BodyCosmetic);
		bodyIndex++;

		if (bodyIndex >= BodyEquipmentScenes.Length) bodyIndex = 0;

		BodyEquipmentButton.Text = BodyEquipmentScenes[bodyIndex].ResourcePath;
		Player.BodyCosmetic = BodyEquipmentScenes[bodyIndex];
	}
}
