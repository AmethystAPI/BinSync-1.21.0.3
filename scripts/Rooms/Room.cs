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
	private Node2D _chestSpawn;
	private Node2D _altarSpawn;
	private List<Node2D> _spawnPoints = new List<Node2D>();

	public override void _Ready() {
		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(SpawnEnemyRpc), SpawnEnemyRpc);
		NetworkPoint.Register(nameof(EndRpc), EndRpc);
		NetworkPoint.Register(nameof(SpawnChestRpc), SpawnChestRpc);
		NetworkPoint.Register(nameof(SpawnAltarRpc), SpawnAltarRpc);
		NetworkPoint.Register(nameof(ActivateRpc), ActivateRpc);

		_barrier = GetNodeOrNull<Barrier>("Barrier");
		Entrance = GetNodeOrNull<Node2D>("Entrance");
		Exit = GetNodeOrNull<Node2D>("Exit");
		_activateArea = GetNodeOrNull<Area2D>("ActivateArea");
		_chestSpawn = GetNodeOrNull<Node2D>("ChestSpawn");
		_altarSpawn = GetNodeOrNull<Node2D>("AltarSpawn");

		if (HasNode("SpawnPoints")) {
			foreach (Node child in GetNodeOrNull<Node2D>("SpawnPoints").GetChildren()) {
				if (!(child is Node2D)) continue;

				_spawnPoints.Add(child as Node2D);
			}
		}

		if (_activateArea != null) _activateArea.BodyEntered += BodyEnteredActivateArea;

		if (!NetworkManager.IsHost) return;

		if (_chestSpawn != null && Game.ShouldSpawnLootRoom()) NetworkPoint.SendRpcToClients(nameof(SpawnChestRpc));

		if (_altarSpawn != null && Game.ShouldSpawnAltar()) NetworkPoint.SendRpcToClients(nameof(SpawnAltarRpc));
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
		SpawnEnemies();
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

	private void SpawnEnemies() {
		if (_spawnPoints.Count == 0) return;

		float points = Game.Difficulty;

		while (points > 0) {
			Node2D spawnPoint = _spawnPoints[new RandomNumberGenerator().RandiRange(0, _spawnPoints.Count - 1)];

			NetworkPoint.SendRpcToClients(nameof(SpawnEnemyRpc), message => {
				message.AddFloat(spawnPoint.GlobalPosition.X);
				message.AddFloat(spawnPoint.GlobalPosition.Y);

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

		if (_chest != null) _chest.Open();
		if (_altar != null) _altar.Activate();

		Game.CompletedRoom();

		NetworkPoint.SendRpcToClients(nameof(EndRpc));
	}

	protected virtual void EndRpc(Message message) {
		_completed = true;

		_barrier.Deactivate();
	}

	private void SpawnChestRpc(Message message) {
		_chest = NetworkManager.SpawnNetworkSafe<Chest>(LootChestScene, "Chest");

		AddChild(_chest);

		_chest.GlobalPosition = _chestSpawn.GlobalPosition;
	}

	private void SpawnAltarRpc(Message message) {
		_altar = NetworkManager.SpawnNetworkSafe<Altar>(ResourceLoader.Load<PackedScene>("scenes/altar.tscn"), "Altar");

		AddChild(_altar);

		_altar.GlobalPosition = _altarSpawn.GlobalPosition;
	}

	private void ActivateRpc(Message message) {
		if (_barrier == null) return;

		_barrier.Deactivate();
	}
}
