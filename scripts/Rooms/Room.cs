using Godot;
using Networking;
using Riptide;
using System.Collections.Generic;

public partial class Room : Node2D, NetworkPointUser {
	[Export] public EnemyPool EnemyPool;
	[Export] public PackedScene LootChestScene;
	[Export] public Vector2 ExitDirection;
	[Export] public bool HasTrinkets = true;

	public Node2D Entrance;
	public Node2D Exit;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private int _aliveEnemies = 0;
	private bool _completed;
	private Chest _chest;
	private Altar _altar;
	private List<Enemy> _spawnedEnemies = new List<Enemy>();
	private Barrier _barrier;
	private Area2D _activateArea;
	private CollisionShape2D _spawnArea;
	private bool _readyToSpawnComponents = true;

	public override void _Ready() {
		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(SpawnEnemyRpc), SpawnEnemyRpc);
		NetworkPoint.Register(nameof(EndRpc), EndRpc);
		NetworkPoint.Register(nameof(ActivateRpc), ActivateRpc);

		_barrier = GetNodeOrNull<Barrier>("Barrier");
		Entrance = GetNodeOrNull<Node2D>("Entrance");
		Exit = GetNodeOrNull<Node2D>("Exit");
		_activateArea = GetNodeOrNull<Area2D>("ActivateArea");
		_spawnArea = GetNodeOrNull<CollisionShape2D>("SpawnArea");

		if (_activateArea != null) _activateArea.BodyEntered += BodyEnteredActivateArea;

		if (!NetworkManager.IsHost) return;
	}

	public override void _PhysicsProcess(double delta) {
		if (_readyToSpawnComponents) {
			_readyToSpawnComponents = false;

			SpawnComponents();
		}
	}

	internal virtual void SpawnComponents() {
		SpawnEnemies();
	}

	public void AddEnemy() {
		_aliveEnemies++;
	}

	public void RemoveEnemy() {
		_aliveEnemies--;

		if (_aliveEnemies != 0) return;

		if (!NetworkManager.IsHost) return;

		Complete();
	}

	public void Place() {
		_readyToSpawnComponents = true;
	}

	public void Activate() {
		WorldGenerator.PlaceRoom();

		NetworkPoint.SendRpcToClients(nameof(ActivateRpc));
	}

	private void BodyEnteredActivateArea(Node2D body) {
		if (!NetworkManager.IsHost) return;

		if (_completed) return;

		if (!(body is Player)) return;

		foreach (Enemy enemy in _spawnedEnemies) {
			enemy.Activate();
		}
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
			CollisionMask = 2
		}); ;
	}

	private void SpawnEnemies() {
		if (_spawnArea == null) return;

		float points = Game.Difficulty;

		while (points > 0) {
			Vector2 spawnPoint = GetRandomPointInSpawnArea();

			while (DetectSpawnOverlap(spawnPoint).Count > 0) {
				spawnPoint = GetRandomPointInSpawnArea();
			}

			NetworkPoint.SendRpcToClients(nameof(SpawnEnemyRpc), message => {
				message.AddFloat(spawnPoint.X);
				message.AddFloat(spawnPoint.Y);

				message.AddInt(new RandomNumberGenerator().RandiRange(0, EnemyPool.EnemyScenes.Length - 1));
			});

			points--;
		}
	}

	private void SpawnEnemyRpc(Message message) {
		Vector2 position = new Vector2(message.GetFloat(), message.GetFloat());
		int enemySceneIndex = message.GetInt();

		Enemy enemy = NetworkManager.SpawnNetworkSafe<Enemy>(EnemyPool.EnemyScenes[enemySceneIndex], "Enemy");

		AddChild(enemy);

		enemy.GlobalPosition = position;

		_spawnedEnemies.Add(enemy);
	}

	protected void Complete() {
		if (!NetworkManager.IsHost) return;

		_completed = true;

		Game.CompletedRoom();

		NetworkPoint.SendRpcToClients(nameof(EndRpc));
	}

	protected virtual void EndRpc(Message message) {
		_completed = true;
	}

	private void ActivateRpc(Message message) {
		if (_barrier == null) return;

		_barrier.Deactivate();
	}
}
