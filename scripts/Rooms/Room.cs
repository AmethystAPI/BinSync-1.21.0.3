using Godot;
using Networking;
using Riptide;
using System.Linq;

public partial class Room : Node2D, NetworkPointUser {
	[Export] public EnemyPool EnemyPool;
	[Export] public PackedScene LootChestScene;
	[Export] public Node2D LootChestSpawn;
	[Export] public Node2D[] SpawnPoints;
	[Export] public Node2D[] Connections;
	[Export] public Vector2[] ConnectionDirections;
	[Export] public bool HasTrinkets = true;
	[Export] public Area2D ActivateArea;
	[Export] public TileMap BarrierTilemap;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private int _aliveEnemies = 0;
	private Vector2 _exitDirection;
	private bool _completed;
	private Chest _chest;

	public override void _Ready() {
		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(SpawnEnemyRpc), SpawnEnemyRpc);
		NetworkPoint.Register(nameof(EndRpc), EndRpc);
		NetworkPoint.Register(nameof(SpawnChestRpc), SpawnChestRpc);

		if (ActivateArea != null) ActivateArea.BodyEntered += BodyEnteredActivateArea;

		if (LootChestSpawn == null) return;

		if (!Game.ShouldSpawnLootRoom()) return;

		NetworkPoint.SendRpcToClients(nameof(SpawnChestRpc));
	}

	public override void _Process(double delta) {
		if (!_completed) return;

		if (BarrierTilemap == null) return;

		Color color = BarrierTilemap.Modulate;
		color.A = 0;

		BarrierTilemap.Modulate = BarrierTilemap.Modulate.Lerp(color, 8f * (float)delta);
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

	public virtual void Place() {
		SpawnEnemies();
	}

	public virtual void PlaceEntrance(Vector2 direction) {

	}

	public virtual void PlaceExit(Vector2 direction) {
		_exitDirection = direction;
	}

	private void BodyEnteredActivateArea(Node2D body) {
		if (!NetworkManager.IsHost) return;

		if (_completed) return;

		if (!(body is Player)) return;

		foreach (Enemy enemy in GetTree().GetNodesInGroup("Enemies")) {
			enemy.Activate();
		}
	}

	private void SpawnEnemies() {
		if (SpawnPoints.Length == 0) return;

		float points = Game.Difficulty;

		while (points > 0) {
			Node2D spawnPoint = SpawnPoints[new RandomNumberGenerator().RandiRange(0, SpawnPoints.Length - 1)];

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

		Node2D enemy = NetworkManager.SpawnNetworkSafe<Node2D>(EnemyPool.EnemyScenes[enemySceneIndex], "Enemy");

		AddChild(enemy);

		enemy.GlobalPosition = position;
	}

	protected void Complete() {
		if (!NetworkManager.IsHost) return;

		_completed = true;

		if (_chest != null) _chest.Open();

		Game.CompletedRoom();

		WorldGenerator.PlaceNextRoom(Connections[ConnectionDirections.ToList().IndexOf(_exitDirection)].GlobalPosition, _exitDirection);

		NetworkPoint.SendRpcToClients(nameof(EndRpc));
	}

	protected virtual void EndRpc(Message message) {
		_completed = true;

		if (BarrierTilemap == null) return;

		Delay.Execute(0.5f, () => {
			BarrierTilemap.SetLayerEnabled(0, false);
		});
	}

	private void SpawnChestRpc(Message message) {
		_chest = NetworkManager.SpawnNetworkSafe<Chest>(LootChestScene, "Chest");

		AddChild(_chest);

		_chest.GlobalPosition = LootChestSpawn.GlobalPosition;
	}
}
