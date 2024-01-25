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

		NetworkPoint.SendRpcToClients(nameof(SpawnLootRpc), message => {
			message.AddInt(new RandomNumberGenerator().RandiRange(0, LootPool.LootScenes.Length - 1));
		});
	}

	private void OpenRpc(Message message) {
		AnimationPlayer.Play("open");
	}

	private void SpawnLootRpc(Message message) {
		int lootSceneIndex = message.GetInt();

		Node2D loot = NetworkManager.SpawnNetworkSafe<Node2D>(LootPool.LootScenes[lootSceneIndex], "Loot");

		AddChild(loot);

		loot.GlobalPosition = GlobalPosition + Vector2.Down * 16f;
	}
}
