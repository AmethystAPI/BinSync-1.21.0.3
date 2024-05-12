using Godot;
using Networking;
using Riptide;
using System.Collections.Generic;

public partial class Game : Node2D, NetworkPointUser {
	public static uint Seed;
	public static float Difficulty;
	public static List<Room> NextRooms = new List<Room>();
	public static RandomNumberGenerator RandomNumberGenerator;
	public static Game Me;
	public static Room CurrentRoom;

	[Export] public PackedScene PlayerScene;
	[Export] public int TrinketRoomInterval = 5;
	[Export] public int LootRoomInterval = 7;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private WorldGenerator _worldGenerator;
	private int _roomsTilTrinket;
	private int _roomsTilLoot;

	public override void _Ready() {
		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(StartRpc), StartRpc);
		NetworkPoint.Register(nameof(CleanupRpc), CleanupRpc);

		Me = this;

		_worldGenerator = GetNode<WorldGenerator>("WorldGenerator");

		NetworkManager.ClientConnected += (ServerConnectedEventArgs eventArguments) => {
			// if (NetworkManager.LocalServer.ClientCount != 2 || eventArguments.Client != NetworkManager.LocalServer.Clients[1]) return;

			Start();
		};

		if (!NetworkManager.Host()) NetworkManager.Join("127.0.0.1");
	}

	public static void Start() {
		Me._roomsTilTrinket = Me.TrinketRoomInterval;
		Me._roomsTilLoot = Me.LootRoomInterval;

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

	public static void CompletedRoom() {
		if (!NetworkManager.IsHost) return;

		Me._roomsTilTrinket--;
		Me._roomsTilLoot--;

		if (Me._roomsTilTrinket <= 0) {
			Me._roomsTilTrinket = Me.TrinketRoomInterval;

			if (Me._roomsTilLoot <= 0) Me._roomsTilLoot = 1;
		}

		if (Me._roomsTilLoot <= 0) Me._roomsTilLoot = Me.LootRoomInterval;

		Difficulty += Mathf.Sqrt(Player.Players.Count) / 3f;

		CurrentRoom = NextRooms[0];
		NextRooms.RemoveAt(0);

		CurrentRoom.Activate();
	}

	public static bool ShouldSpawnAltar() {
		return Me._roomsTilTrinket == 1;
	}

	public static bool ShouldSpawnLootRoom() {
		return Me._roomsTilLoot == 1 && Me._roomsTilTrinket != 1;
	}

	private void StartRpc(Message message) {
		int[] clientIds = message.GetInts();

		NextRooms = new List<Room>();

		_worldGenerator.Start();

		foreach (int clientId in clientIds) {
			Player player = NetworkManager.SpawnNetworkSafe<Player>(PlayerScene, "Player", clientId);

			AddChild(player);
		}
	}

	private void CleanupRpc(Message message) {
		while (Player.Players.Count > 0) {
			Player.Players[0].Cleanup();
		}

		_worldGenerator.Cleanup();
	}
}
