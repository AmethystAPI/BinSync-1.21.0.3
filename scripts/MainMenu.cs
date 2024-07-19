using System;
using Godot;
using Networking;

public partial class MainMenu : Control {
	[Export] public Button hostButton;
	[Export] public Button startButton;
	[Export] public Button HatEquipmentButton;
	[Export] public Button BodyEquipmentButton;

	[Export] public PackedScene[] HatEquipmentScenes;
	[Export] public PackedScene[] BodyEquipmentScenes;

	public override void _Ready() {
		Player.HatCosmetic = HatEquipmentScenes[0];
		Player.BodyCosmetic = BodyEquipmentScenes[0];

		NetworkManager.JoinedServer += () => {
			QueueFree();
		};
	}

	public void Host() {
		NetworkManager.Host();

		hostButton.QueueFree();
	}

	public void Start() {
		if (!NetworkManager.IsHost) return;

		Game.Start();

		QueueFree();
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
