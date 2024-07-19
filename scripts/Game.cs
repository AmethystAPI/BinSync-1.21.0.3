using Godot;
using Networking;
using Riptide;
using Steamworks;
using System;
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

	private Callback<LobbyCreated_t> _lobbyCreatedCallback;
	private Callback<LobbyEnter_t> _lobbyEnteredCallback;
	private Callback<GameLobbyJoinRequested_t> _gameLobbyJoinRequestedCallback;
	private Callback<LobbyChatUpdate_t> _lobbyChatUpdateCallback;

	public override void _Ready() {
		if (!SteamAPI.Init()) {
			GD.PushError("SteamAPI.Init() failed!");

			return;
		}

		_lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(LobbyCreated);
		_lobbyEnteredCallback = Callback<LobbyEnter_t>.Create(LobbyEntered);
		_gameLobbyJoinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create(GameLobbyJoinRequested);
		_lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(LobbyChatUpdated);

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

			SteamAPICall_t handle = SteamMatchmaking.RequestLobbyList();
			CallResult<LobbyMatchList_t> callResult = CallResult<LobbyMatchList_t>.Create(LobbiesMatched);
			callResult.Set(handle);
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

		SteamMatchmaking.SetLobbyData((CSteamID)lobbyCreated.m_ulSteamIDLobby, "name", "Project Squad Test Lobby");

		GD.Print("Set joinable!");
	}

	private void LobbyEntered(LobbyEnter_t lobbyEntered) {
		GD.Print("Entered lobby! " + SteamMatchmaking.GetLobbyData((CSteamID)lobbyEntered.m_ulSteamIDLobby, "name"));
	}

	private void GameLobbyJoinRequested(GameLobbyJoinRequested_t gameLobbyJoinRequested) {
		GD.Print("Requested join lobby! " + SteamMatchmaking.GetLobbyData(gameLobbyJoinRequested.m_steamIDLobby, "name"));

		SteamMatchmaking.JoinLobby(gameLobbyJoinRequested.m_steamIDLobby);
	}

	private void LobbiesMatched(LobbyMatchList_t lobbyMatchList, bool bIOFailure) {
		GD.Print("Lobbies matched: " + lobbyMatchList.m_nLobbiesMatching);

		uint count = lobbyMatchList.m_nLobbiesMatching;

		for (int index = 0; index < count; index++) {
			CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(index);
			string name = SteamMatchmaking.GetLobbyData(lobbyId, "name");

			GD.Print("Name: " + name);
		}
	}

	private void LobbyChatUpdated(LobbyChatUpdate_t lobbyChatUpdate) {
		GD.Print("Lobby Updated: " + SteamMatchmaking.GetNumLobbyMembers((CSteamID)lobbyChatUpdate.m_ulSteamIDLobby));
	}
}
