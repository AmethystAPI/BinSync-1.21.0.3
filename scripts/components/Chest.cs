using Godot;
using Networking;
using Riptide;

public partial class Chest : Node2D, NetworkPointUser {
	[Export] public LootPool LootPool;
	[Export] public AnimationPlayer AnimationPlayer;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	public override void _Ready() {
		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(SpawnLootRpc), SpawnLootRpc);
		NetworkPoint.Register(nameof(OpenRpc), OpenRpc);
	}

	public void Open() {
		NetworkPoint.SendRpcToClients(nameof(OpenRpc));

		float angleOffset = Mathf.Min(Mathf.Pi / 3, 2f * Mathf.Pi / Player.Players.Count);

		float startAngle = (Player.Players.Count / 2f - 0.5f) * -angleOffset;
		if (Player.Players.Count % 2 == 1) startAngle = Mathf.Floor(Player.Players.Count / 2f) * -angleOffset;

		for (int index = 0; index < Player.Players.Count; index++) {
			NetworkPoint.SendRpcToClients(nameof(SpawnLootRpc), message => {
				message.AddInt(new RandomNumberGenerator().RandiRange(0, LootPool.LootScenes.Length - 1));
				message.AddFloat(startAngle + angleOffset * index);
			});
		}
	}

	private void OpenRpc(Message message) {
		AnimationPlayer.Play("open");
	}

	private void SpawnLootRpc(Message message) {
		int lootSceneIndex = message.GetInt();
		float angle = message.GetFloat();

		Node2D loot = NetworkManager.SpawnNetworkSafe<Node2D>(LootPool.LootScenes[lootSceneIndex], "Loot");

		AddChild(loot);

		loot.GlobalPosition = GlobalPosition + Vector2.Down.Rotated(angle) * 16f;
	}
}
