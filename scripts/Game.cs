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

	public static bool DEBUG_MAIN = false;

	private Callback<LobbyCreated_t> _createLobbyCallback;
	private Callback<LobbyMatchList_t> _requestLobbyListCallback;

	public override void _Ready() {
		if (!SteamAPI.Init()) {
			GD.PushError("SteamAPI.Init() failed!");

			return;
		}

		_createLobbyCallback = Callback<LobbyCreated_t>.Create(LobbyCreated);
		_requestLobbyListCallback = Callback<LobbyMatchList_t>.Create(LobbiesMatched);

		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(StartRpc), StartRpc);
		NetworkPoint.Register(nameof(CleanupRpc), CleanupRpc);

		Me = this;

		_worldGenerator = GetNode<WorldGenerator>("WorldGenerator");

		DEBUG_MAIN = SteamFriends.GetPersonaName() == "Outer Cloud Studio";

		if (DEBUG_MAIN) {
			GD.Print("Creating lobby...");

			SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 16);

		} else {
			GD.Print("Searching for lobbies...");

			SteamMatchmaking.RequestLobbyList();
		}

		// NetworkManager.ClientConnected += (ServerConnectedEventArgs eventArguments) => {
		// 	if (NetworkManager.LocalServer.ClientCount != 2 || eventArguments.Client != NetworkManager.LocalServer.Clients[1]) return;

		// 	Start();
		// };

		// if (!NetworkManager.Host()) NetworkManager.Join("localhost");
	}

	public override void _Process(double delta) {
		SteamAPI.RunCallbacks();
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
		Difficulty += Mathf.Sqrt(Player.Players.Count) / 3f;
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

		Room.Cleanup();
	}

	private void LobbyCreated(LobbyCreated_t lobbyCreated) {
		GD.Print("Created lobby! " + (lobbyCreated.m_eResult == EResult.k_EResultOK));

		SteamMatchmaking.SetLobbyJoinable((CSteamID)lobbyCreated.m_ulSteamIDLobby, true);

		GD.Print("Set joinable!");
	}

	private void LobbiesMatched(LobbyMatchList_t lobbyMatchList) {
		GD.Print("Lobbies matched: " + lobbyMatchList.m_nLobbiesMatching);
	}
}
