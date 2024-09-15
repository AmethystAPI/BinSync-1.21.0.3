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
	private PackedScene _roomClearEffect;
	private float _pointsTillNextRound;
	private List<List<int>> _rounds = new List<List<int>>();
	private float _pointsPerRound;
	private float _pointsCollected;
	private List<Node2D> _spawningEnemies = new List<Node2D>();

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

		_roomClearEffect = ResourceLoader.Load<PackedScene>("res://scenes/rooms/room_clear_effect.tscn");

		if (!NetworkManager.IsHost) return;

		GetTree().ProcessFrame += DelaySpawnComponents;
	}

	public override void _Process(double delta) {
		if (!NetworkManager.IsHost) return;

		if (_completed) return;


	}

	public static void CleanupAll() {
		foreach (Room room in s_Rooms) {
			room.Cleanup();
		}

		s_Rooms.Clear();
	}

	public void Cleanup() {
		foreach (Node2D node in _spawnedEnemies) {
			node.QueueFree();
		}

		QueueFree();
	}

	public void AddEnemy() {
		_aliveEnemies++;
	}

	public void EnemyDied(Enemy enemy) {
		_aliveEnemies--;

		_pointsCollected += enemy.Points;

		if (_pointsCollected >= _pointsPerRound * 0.8f && _rounds.Count > 0) {
			_pointsCollected -= _pointsCollected;

			List<int> round = _rounds[0];
			_rounds.RemoveAt(0);

			SpawnEnemiesFromRound(round, true, true);

			return;
		}

		if (_rounds.Count != 0 || _aliveEnemies != 0) return;

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

	private Vector2 GetRandomPointAroundPlayer() {
		Player player = Player.Players[Game.RandomNumberGenerator.RandiRange(0, Player.AlivePlayers.Count - 1)];

		return player.GlobalPosition + Vector2.Right * Game.RandomNumberGenerator.RandfRange(-64, 64) + Vector2.Up * Game.RandomNumberGenerator.RandfRange(-64, 64);
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

		int rounds = Mathf.FloorToInt(Mathf.Pow(points / 4f, 0.8f));
		if (rounds < 1) rounds = 1;
		_pointsPerRound = points / rounds;
		_pointsCollected = 0;

		while (points > 0) {
			float pointsForThisRound = _pointsPerRound;

			List<int> round = new List<int>();

			while (pointsForThisRound > 0) {
				int selectedEnemyIndex = new RandomNumberGenerator().RandiRange(0, EnemyPool.EnemyScenes.Length - 1);

				round.Add(selectedEnemyIndex);

				points -= EnemyPool.Points[selectedEnemyIndex];
				pointsForThisRound -= EnemyPool.Points[selectedEnemyIndex];

				if (EnemyPool.Points[selectedEnemyIndex] == 0) points -= 1;
				if (EnemyPool.Points[selectedEnemyIndex] == 0) pointsForThisRound -= 1;
			}

			_rounds.Add(round);
		}

		List<int> firstRound = _rounds[0];
		_rounds.RemoveAt(0);

		SpawnEnemiesFromRound(firstRound, false, activated);
	}

	private void SpawnEnemiesFromRound(List<int> round, bool spawnAroundPlayers = false, bool activated = false) {
		foreach (int enemyTypeIndex in round) {
			Vector2 spawnPoint = spawnAroundPlayers ? GetRandomPointAroundPlayer() : GetRandomPointInSpawnArea();

			while (DetectSpawnOverlap(spawnPoint).Count > 0) {
				spawnPoint = spawnAroundPlayers ? GetRandomPointAroundPlayer() : GetRandomPointInSpawnArea();
			}

			AddEnemy();

			NetworkPoint.SendRpcToClients(nameof(SpawnEnemyRpc), message => {
				message.AddBool(spawnAroundPlayers);
				message.AddFloat(EnemyPool.Points[enemyTypeIndex]);
				message.AddFloat(spawnPoint.X);
				message.AddFloat(spawnPoint.Y);

				message.AddInt(enemyTypeIndex);

				message.AddBool(activated);
			});
		}
	}

	private void SpawnEnemyRpc(Message message) {
		bool spawnInstantly = !message.GetBool();
		float points = message.GetFloat();
		Vector2 position = new Vector2(message.GetFloat(), message.GetFloat());
		int enemySceneIndex = message.GetInt();
		bool activated = message.GetBool();

		Enemy enemy = NetworkManager.SpawnNetworkSafe<Enemy>(EnemyPool.EnemyScenes[enemySceneIndex], "Enemy");
		enemy.Points = points;

		if (enemy.Points == 0) enemy.Points = 0;

		if (spawnInstantly) {
			AddChild(enemy);

			enemy.GlobalPosition = position;

			_spawnedEnemies.Add(enemy);

			if (activated) enemy.Activate();
		} else {
			PackedScene spawnDustScene = ResourceLoader.Load<PackedScene>("res://scenes/particles/spawn_dust.tscn");
			Node2D spawnDust = spawnDustScene.Instantiate<Node2D>();

			AddChild(spawnDust);

			spawnDust.GlobalPosition = position;

			_spawningEnemies.Add(enemy);

			Delay.Execute(1, () => {
				if (!IsInstanceValid(this)) return;

				AddChild(enemy);

				enemy.GlobalPosition = position;

				_spawnedEnemies.Add(enemy);

				if (activated) enemy.Activate();

				_spawningEnemies.Remove(enemy);
			});
		}
	}

	protected virtual void Complete(Enemy enemy = null) {
		if (enemy != null) {
			Node2D roomClearEffect = _roomClearEffect.Instantiate<Node2D>();
			AddChild(roomClearEffect);
			roomClearEffect.GlobalPosition = enemy.GlobalPosition;
			Delay.Execute(1f, () => {
				roomClearEffect.QueueFree();
			});
		}

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
