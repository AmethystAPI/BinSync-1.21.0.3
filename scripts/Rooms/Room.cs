using Godot;
using Networking;
using Riptide;
using System.Collections.Generic;

public partial class Room : Node2D, NetworkPointUser {

	[Export] public EnemyPool EnemyPool;
	[Export] public Vector2 ExitDirection;

	public static Room Current;

	private static List<Room> s_Rooms = new List<Room>();

	public Node2D Entrance;
	public Node2D Exit;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	protected List<Enemy> _spawnedEnemies = new List<Enemy>();

	private bool _completed;
	private int _aliveEnemies = 0;
	private Room _nextRoom;

	private Barrier _barrier;
	private Area2D _activateArea;
	private CollisionShape2D _spawnArea;

	public void Load() {
		Entrance = GetNodeOrNull<Node2D>("Entrance");
		Exit = GetNodeOrNull<Node2D>("Exit");
	}

	public override void _Ready() {
		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(SpawnEnemyRpc), SpawnEnemyRpc);
		NetworkPoint.Register(nameof(ActivateRpc), ActivateRpc);
		NetworkPoint.Register(nameof(ActivateEnemiesRpc), ActivateEnemiesRpc);

		_barrier = GetNodeOrNull<Barrier>("Barrier");

		Load();

		_activateArea = GetNodeOrNull<Area2D>("ActivateArea");
		_spawnArea = GetNodeOrNull<CollisionShape2D>("SpawnArea");

		if (_activateArea != null) _activateArea.BodyEntered += BodyEnteredActivateArea;

		s_Rooms.Add(this);

		if (!NetworkManager.IsHost) return;

		GetTree().ProcessFrame += DelaySpawnComponents;
	}

	public override void _Process(double delta) {
		if (!NetworkManager.IsHost) return;

		if (_completed) return;


	}

	public static void Cleanup() {
		foreach (Room room in s_Rooms) {
			room.QueueFree();
		}

		s_Rooms.Clear();
	}

	public void AddEnemy() {
		_aliveEnemies++;
	}

	public void EnemyDied(Enemy enemy) {
		_aliveEnemies--;

		if (_aliveEnemies != 0) return;

		if (!NetworkManager.IsHost) return;

		Complete(enemy);
	}

	public void Activate() {
		if (!NetworkManager.IsHost) return;

		WorldGenerator.PlaceRoom(this);

		NetworkPoint.SendRpcToClients(nameof(ActivateRpc));
	}

	public void ActivateEnemies() {
		foreach (Enemy enemy in _spawnedEnemies) {
			enemy.Activate();
		}
	}

	public virtual void SetNextRoom(Room nextRoom) {
		_nextRoom = nextRoom;
	}

	private void DelaySpawnComponents() {
		GetTree().ProcessFrame -= DelaySpawnComponents;

		SpawnComponents();
	}

	protected virtual void SpawnComponents() {
		SpawnEnemies(Game.Difficulty);
	}

	private void BodyEnteredActivateArea(Node2D body) {
		if (!NetworkManager.IsHost) return;

		if (_completed) return;

		if (!(body is Player)) return;

		ActivateEnemies();

		NetworkPoint.BounceRpcToClientsFast(nameof(ActivateEnemiesRpc));
	}

	private Vector2 GetRandomPointInSpawnArea() {
		Vector2 position = _spawnArea.GlobalPosition;

		RectangleShape2D shape = _spawnArea.Shape as RectangleShape2D;

		position += Vector2.Right * Game.RandomNumberGenerator.RandfRange(-shape.Size.X / 2, shape.Size.X / 2) + Vector2.Up * Game.RandomNumberGenerator.RandfRange(-shape.Size.Y / 2, shape.Size.Y / 2);

		return position;
	}

	private Godot.Collections.Array<Godot.Collections.Dictionary> DetectSpawnOverlap(Vector2 position) {
		return GetWorld2D().DirectSpaceState.IntersectShape(new PhysicsShapeQueryParameters2D {
			Shape = new CircleShape2D() {
				Radius = 12f,
			},
			Transform = new Transform2D(0f, position),
			CollisionMask = 2,
		});
	}

	public void SpawnEnemies(float points, bool activated = false) {
		if (_spawnArea == null) return;

		while (points > 0) {
			Vector2 spawnPoint = GetRandomPointInSpawnArea();

			while (DetectSpawnOverlap(spawnPoint).Count > 0) {
				spawnPoint = GetRandomPointInSpawnArea();
			}

			NetworkPoint.SendRpcToClients(nameof(SpawnEnemyRpc), message => {
				message.AddFloat(spawnPoint.X);
				message.AddFloat(spawnPoint.Y);

				message.AddInt(new RandomNumberGenerator().RandiRange(0, EnemyPool.EnemyScenes.Length - 1));

				message.AddBool(activated);
			});

			points--;
		}
	}

	private void SpawnEnemyRpc(Message message) {
		Vector2 position = new Vector2(message.GetFloat(), message.GetFloat());
		int enemySceneIndex = message.GetInt();
		bool activated = message.GetBool();

		Enemy enemy = NetworkManager.SpawnNetworkSafe<Enemy>(EnemyPool.EnemyScenes[enemySceneIndex], "Enemy");

		AddChild(enemy);

		enemy.GlobalPosition = position;

		if (position.DistanceTo(_spawnArea.GlobalPosition) > 120f) GD.PushWarning("Spawned enemy at distance " + position.DistanceTo(_spawnArea.GlobalPosition));

		_spawnedEnemies.Add(enemy);

		if (activated) enemy.Activate();
	}

	protected virtual void Complete(Enemy enemy = null) {
		if (!NetworkManager.IsHost) return;

		_completed = true;

		Game.IncreaseDifficulty();

		_nextRoom.Activate();
	}

	protected virtual void ActivateRpc(Message message) {
		Current = this;

		if (_barrier == null) return;

		_barrier.Deactivate();
	}

	protected virtual void ActivateEnemiesRpc(Message message) {

	}
}
