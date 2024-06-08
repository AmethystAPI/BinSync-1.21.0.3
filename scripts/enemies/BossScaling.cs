using Godot;
using Networking;
using Riptide;

public partial class BossScaling : Node2D, NetworkPointUser {
	[Export] public float DifficultyScaling = 1f;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	public override void _Ready() {
		NetworkPoint.Setup(this);
		NetworkPoint.Register(nameof(UpdateHealthRpc), UpdateHealthRpc);

		if (!NetworkManager.IsHost) return;

		Enemy enemy = GetParent<Enemy>();

		NetworkPoint.SendRpcToClients(nameof(UpdateHealthRpc), message => message.AddFloat(enemy.Health * Player.Players.Count + Game.Difficulty * DifficultyScaling));
	}

	private void UpdateHealthRpc(Message message) {
		Enemy enemy = GetParent<Enemy>();
		enemy.Health = message.GetFloat();
	}
}
