using Godot;
using Networking;
using Riptide;
using Steamworks;
using System.Collections.Generic;

public partial class Game : Node2D, NetworkPointUser {
	public static uint Seed;
	public static float Difficulty;
	public static RandomNumberGenerator RandomNumberGenerator;
	public static Game Me;

	[Export] public PackedScene PlayerScene;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private WorldGenerator _worldGenerator;

	public override void _Ready() {
		if (!SteamAPI.Init()) {
			GD.PushError("SteamAPI.Init() failed!");

			return;
		}

		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(StartRpc), StartRpc);
		NetworkPoint.Register(nameof(CleanupRpc), CleanupRpc);

		Me = this;

		_worldGenerator = GetNode<WorldGenerator>("WorldGenerator");

		if (OS.HasFeature("host")) {
			NetworkManager.HostLocal();

			NetworkManager.ClientConnected += (ServerConnectedEventArgs eventArguments) => {
				if (!NetworkManager.IsHost) return;

				if (!OS.HasFeature("single_player")) {
					if (NetworkManager.LocalServer.ClientCount != 2) return;
				}

				Start();
			};
		}

		if (OS.HasFeature("client")) NetworkManager.JoinLocal();
	}

	public override void _Process(double delta) {
		SteamAPI.RunCallbacks();

		if (Input.IsActionJustPressed("fullscreen")) {
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		}
	}

	public static void Start() {
		List<int> clientIds = new List<int>();

		foreach (Connection connection in NetworkManager.LocalServer.Clients) {
			clientIds.Add(connection.Id);
		}

		Seed = new RandomNumberGenerator().Randi();

		RandomNumberGenerator = new RandomNumberGenerator {
			Seed = Seed
		};

		Difficulty = clientIds.Count;

		Me.NetworkPoint.SendRpcToClients(nameof(StartRpc), message => {
			message.AddInts(clientIds.ToArray());
		});
	}

	public static void Restart() {
		Me.NetworkPoint.SendRpcToClients(nameof(CleanupRpc));

		Start();
	}

	public static void IncreaseDifficulty() {
		Difficulty += Mathf.Sqrt(Player.Players.Count) / 2f;
	}

	private void StartRpc(Message message) {
		int[] clientIds = message.GetInts();

		_worldGenerator.Start();

		foreach (int clientId in clientIds) {
			Player player = NetworkManager.SpawnNetworkSafe<Player>(PlayerScene, "Player", clientId);

			AddChild(player);
		}

		Audio.PlayMusic("golden_grove");
	}

	private void CleanupRpc(Message message) {
		while (Player.Players.Count > 0) {
			Player.Players[0].Cleanup();
		}

		Room.CleanupAll();
	}
}
