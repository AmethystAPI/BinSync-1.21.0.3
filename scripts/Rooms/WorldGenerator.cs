using Godot;
using Networking;
using Riptide;
using System.Collections.Generic;
using System.Linq;

public partial class WorldGenerator : Node2D, NetworkPointUser {
	private static WorldGenerator s_Me;

	[Export] public Biome[] Biomes;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private int _biomeLevel = 0;
	private Biome _currentBiome;
	private List<UniqueEncounter.State> _uniqueEncounterStates = new List<UniqueEncounter.State>();
	private int _roomsTillNewBiome = 0;

	public override void _Ready() {
		s_Me = this;

		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(PlaceRoomRpc), PlaceRoomRpc);
	}

	public void Start() {
		if (NetworkManager.IsHost) {
			_biomeLevel = 0;

			SetupBiome();

			PlaceSpawnRoom();
		}
	}

	private void SetupBiome() {
		Biome[] possibleBiomes = Biomes.Where(biome => biome.Level == _biomeLevel).ToArray();

		_currentBiome = possibleBiomes[Game.RandomNumberGenerator.RandiRange(0, possibleBiomes.Length - 1)];

		_uniqueEncounterStates.Clear();

		foreach (UniqueEncounter uniqueEncounter in _currentBiome.UniqueEncounters) {
			_uniqueEncounterStates.Add(uniqueEncounter.GetState(Game.RandomNumberGenerator));
		}

		_uniqueEncounterStates = _uniqueEncounterStates.OrderByDescending(state => state.Source.Priority).ToList();

		_roomsTillNewBiome = Game.RandomNumberGenerator.RandiRange(_currentBiome.Size.X, _currentBiome.Size.Y);
	}

	private RoomPlacer SelectRoom(Room sourceRoom) {
		if (_roomsTillNewBiome <= 0) {
			_biomeLevel++;

			SetupBiome();
		}

		_roomsTillNewBiome--;

		List<UniqueEncounter.State> validStates = _uniqueEncounterStates.Where(state => {
			if (state.Source.Limit != 0 && state.Placed >= state.Source.Limit) return false;

			if (state.RoomsTillPlace > 0) return false;

			if (sourceRoom != null && !state.Source.RoomPlacer.CanConnectTo(sourceRoom.ExitDirection)) return false;

			return true;
		}).ToList();

		if (validStates.Count > 0 && _roomsTillNewBiome != 0) {
			UniqueEncounter.State chosenState = validStates[0];

			int index = _uniqueEncounterStates.IndexOf(chosenState);

			_uniqueEncounterStates[index] = chosenState.Source.GetState(Game.RandomNumberGenerator);
			_uniqueEncounterStates[index].Placed = chosenState.Placed + 1;

			foreach (UniqueEncounter.State state in _uniqueEncounterStates) {
				state.RoomsTillPlace--;
			}

			return chosenState.Source.RoomPlacer;
		}

		RoomPlacer[] validRoomPlacers = _currentBiome.RoomPlacers.Where(placer => {
			if (!placer.CanConnectTo(sourceRoom.ExitDirection)) return false;

			if (sourceRoom.ExitDirection != Vector2.Up && placer.CanConnectTo(Vector2.Up)) return false;

			if (_roomsTillNewBiome == 0 && placer.ExitDirection != Vector2.Up) return false;

			return true;
		}).ToArray();

		if (_roomsTillNewBiome != 0) {
			List<UniqueEncounter.State> expectedStates = _uniqueEncounterStates.Where(state => {
				if (state.Source.Limit != 0 && state.Placed >= state.Source.Limit) return false;

				if (state.RoomsTillPlace > 1) return false;

				return true;
			}).ToList();

			foreach (UniqueEncounter.State expectedState in expectedStates) {
				RoomPlacer[] newValidRoomPlacers = validRoomPlacers.Where(roomPlacer => expectedState.Source.RoomPlacer.CanConnectTo(roomPlacer.ExitDirection)).ToArray();

				if (newValidRoomPlacers.Length > 0) {
					validRoomPlacers = newValidRoomPlacers;

					break;
				}
			}
		}

		foreach (UniqueEncounter.State state in _uniqueEncounterStates) {
			state.RoomsTillPlace--;
		}

		return validRoomPlacers[Game.RandomNumberGenerator.RandiRange(0, validRoomPlacers.Length - 1)];
	}

	private void PlaceSpawnRoom() {
		s_Me.NetworkPoint.SendRpcToClients(nameof(PlaceRoomRpc), message => {
			message.AddString(SelectRoom(null).ResourcePath);
			message.AddString("");
			message.AddFloat(0);
			message.AddFloat(0);
		});
	}

	public static void PlaceRoom(Room sourceRoom) {
		if (!NetworkManager.IsHost) return;

		RoomPlacer roomPlacer = s_Me.SelectRoom(sourceRoom);
		Room room = roomPlacer.RoomScene.Instantiate<Room>();
		room.Load();

		Vector2 placePosition = sourceRoom.Exit.GlobalPosition - room.Entrance.Position;

		room.QueueFree();

		s_Me.NetworkPoint.SendRpcToClients(nameof(PlaceRoomRpc), message => {
			message.AddString(roomPlacer.ResourcePath);
			message.AddString(sourceRoom.GetPath());
			message.AddFloat(placePosition.X);
			message.AddFloat(placePosition.Y);
		});
	}

	private void PlaceRoomRpc(Message message) {
		string roomPlacerPath = message.GetString();
		string sourceRoomPath = message.GetString();
		Vector2 placePosition = new Vector2(message.GetFloat(), message.GetFloat());

		RoomPlacer roomPlacer = ResourceLoader.Load<RoomPlacer>(roomPlacerPath);

		Room room = NetworkManager.SpawnNetworkSafe<Room>(roomPlacer.RoomScene, "Room");

		room.GlobalPosition = placePosition;

		AddChild(room);

		if (sourceRoomPath != "") GetTree().Root.GetNode<Room>(sourceRoomPath).SetNextRoom(room);
	}
}
