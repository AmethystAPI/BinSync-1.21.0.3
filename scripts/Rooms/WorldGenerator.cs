using Godot;
using Networking;
using Riptide;
using System.Linq;

public partial class WorldGenerator : Node2D, NetworkPointUser {
	private static WorldGenerator s_Me;

	[Export] public RoomPlacer SpawnRoomPlacer;
	[Export] public RoomPlacer[] RoomPlacers;
	[Export] public RoomPlacer TempleRoomPlacer;
	[Export] public Vector2I TempleRoomInterval = new Vector2I(5, 8);
	[Export] public RoomPlacer BossRoomPlacer;
	[Export] public Vector2I BossRoomInterval = new Vector2I(15, 20);

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private int _roomsTilTemple;
	private int _roomsTilBoss;

	public override void _Ready() {
		s_Me = this;

		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(PlaceRoomRpc), PlaceRoomRpc);
	}

	public void Start() {
		if (NetworkManager.IsHost) {
			_roomsTilTemple = Game.RandomNumberGenerator.RandiRange(TempleRoomInterval.X, TempleRoomInterval.Y);
			_roomsTilBoss = Game.RandomNumberGenerator.RandiRange(BossRoomInterval.X, BossRoomInterval.Y);
		}

		PlaceSpawnRoom();
	}

	private void PlaceSpawnRoom() {
		SpawnRoom spawnRoom = NetworkManager.SpawnNetworkSafe<SpawnRoom>(SpawnRoomPlacer.RoomScene, "SpawnRoom");

		AddChild(spawnRoom);
	}

	public static void PlaceRoom(Room sourceRoom) {
		if (!NetworkManager.IsHost) return;

		s_Me._roomsTilTemple--;
		s_Me._roomsTilBoss--;

		RoomPlacer[] validRoomPlacers = s_Me.RoomPlacers.Where(placer => {
			if (!placer.CanConnectTo(sourceRoom.ExitDirection)) return false;

			if (sourceRoom.ExitDirection != Vector2.Up && placer.CanConnectTo(Vector2.Up)) return false;

			return true;
		}).ToArray();

		if (s_Me._roomsTilTemple == 0) {
			validRoomPlacers = new RoomPlacer[] { s_Me.TempleRoomPlacer };

			s_Me._roomsTilTemple = Game.RandomNumberGenerator.RandiRange(s_Me.TempleRoomInterval.X, s_Me.TempleRoomInterval.Y);
		}

		if (s_Me._roomsTilBoss == 0) {
			validRoomPlacers = new RoomPlacer[] { s_Me.BossRoomPlacer };

			s_Me._roomsTilBoss = Game.RandomNumberGenerator.RandiRange(s_Me.BossRoomInterval.X, s_Me.BossRoomInterval.Y);
		}

		if (s_Me._roomsTilBoss == 1 || s_Me._roomsTilTemple == 1) {
			validRoomPlacers = s_Me.RoomPlacers.Where(placer => {
				if (!placer.CanConnectTo(sourceRoom.ExitDirection)) return false;

				if (!placer.CanConnectTo(Vector2.Down)) return false;

				if (sourceRoom.ExitDirection != Vector2.Up && placer.CanConnectTo(Vector2.Up)) return false;

				return true;
			}).ToArray();
		}

		RoomPlacer roomPlacer = validRoomPlacers[Game.RandomNumberGenerator.RandiRange(0, validRoomPlacers.Length - 1)];

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

		GetTree().Root.GetNode<Room>(sourceRoomPath).SetNextRoom(room);
	}
}
