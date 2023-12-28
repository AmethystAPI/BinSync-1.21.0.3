using Godot;
using Networking;
using Riptide;
using System;
using System.Linq;

public partial class Room : Node2D, NetworkPointUser
{
	[Export] public PackedScene[] EnemyScenes;
	[Export] public PackedScene[] LootScenes;
	[Export] public Node2D[] SpawnPoints;
	[Export] public Vector2[] EdgeTileMapDirections;
	[Export] public TileMap[] EdgeTileMaps;
	[Export] public Vector2[] EntranceDirections;
	[Export] public Node2D[] Entrances;
	[Export] public Vector2[] ExitDirections;
	[Export] public Node2D[] Exits;

	public Action Started;
	public Action Completed;
	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private bool _spawned = false;
	private int _aliveEnemies = 0;
	private int _playersEntered = 0;
	private bool _started = false;

	public override void _Ready()
	{
		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(StartRpc), StartRpc);
		NetworkPoint.Register(nameof(EndRpc), EndRpc);
		NetworkPoint.Register(nameof(SpawnEnemyRpc), SpawnEnemyRpc);
		NetworkPoint.Register(nameof(SpawnLootRpc), SpawnLootRpc);

		foreach (Node2D entrance in Entrances)
		{
			entrance.GetParent().RemoveChild(entrance);
		}

		foreach (Node2D exit in Exits)
		{
			exit.GetParent().RemoveChild(exit);
		}

		if (!NetworkManager.IsHost) return;

		Area2D spawnTriggerArea = GetNode<Area2D>("SpawnTriggerArea");

		spawnTriggerArea.BodyEntered += OnBodyEntered;
		spawnTriggerArea.BodyExited += OnBodyExited;
	}

	public virtual void PlaceEntrance(Vector2 direction)
	{
		EdgeTileMaps[EdgeTileMapDirections.ToList().IndexOf(direction)].QueueFree();

		AddChild(Entrances[EntranceDirections.ToList().IndexOf(direction)]);
	}

	public virtual void PlaceExit(Vector2 direction)
	{
		EdgeTileMaps[EdgeTileMapDirections.ToList().IndexOf(direction)].QueueFree();

		AddChild(Exits[ExitDirections.ToList().IndexOf(direction)]);
	}

	public virtual void Place()
	{
		foreach (Node2D entrance in Entrances)
		{
			if (entrance.IsInsideTree()) continue;
			entrance.QueueFree();
		}

		foreach (Node2D exit in Exits)
		{
			if (exit.IsInsideTree()) continue;
			exit.QueueFree();
		}
	}

	public void AddEnemy()
	{
		_aliveEnemies++;
	}

	public void RemoveEnemy()
	{
		_aliveEnemies--;

		if (_aliveEnemies != 0) return;

		if (!NetworkManager.IsHost) return;

		NetworkPoint.SendRpcToClients(nameof(EndRpc));
	}

	private void OnBodyEntered(Node2D body)
	{
		if (!(body is Player)) return;

		_playersEntered++;

		if (_playersEntered != Player.Players.Count) return;

		if (_started) return;

		_started = true;

		NetworkPoint.SendRpcToClients(nameof(StartRpc));
	}

	private void OnBodyExited(Node2D body)
	{
		if (!(body is Player)) return;

		_playersEntered--;
	}

	private void StartRpc(Message message)
	{
		Start();
	}

	protected virtual void Start()
	{
		Started?.Invoke();

		WorldGenerator.DespawnLastRoom();

		if (!NetworkManager.IsHost) return;

		SpawnEnemies();
	}

	private void SpawnEnemies()
	{
		if (_spawned) return;

		_spawned = true;

		foreach (Node2D spawnPoint in SpawnPoints)
		{
			NetworkPoint.SendRpcToClients(nameof(SpawnEnemyRpc), message =>
			{
				message.AddFloat(spawnPoint.GlobalPosition.X);
				message.AddFloat(spawnPoint.GlobalPosition.Y);

				message.AddInt(new RandomNumberGenerator().RandiRange(0, EnemyScenes.Length - 1));
			});
		}
	}

	private void SpawnEnemyRpc(Message message)
	{
		Vector2 position = new Vector2(message.GetFloat(), message.GetFloat());
		int enemySceneIndex = message.GetInt();

		Node2D enemy = NetworkManager.SpawnNetworkSafe<Node2D>(EnemyScenes[enemySceneIndex], "Enemy");

		AddChild(enemy);

		enemy.GlobalPosition = position;
	}

	protected virtual void EndRpc(Message message)
	{
		Completed?.Invoke();

		if (!NetworkManager.IsHost) return;

		if (LootScenes.Length == 0) return;

		NetworkPoint.SendRpcToClients(nameof(SpawnLootRpc), message =>
		{
			message.AddString(LootScenes[new RandomNumberGenerator().RandiRange(0, LootScenes.Length - 1)].ResourcePath);
		});
	}

	protected void SpawnLootRpc(Message message)
	{
		string lootScenePath = message.GetString();

		PackedScene lootScene = ResourceLoader.Load<PackedScene>(lootScenePath);

		Node2D item = NetworkManager.SpawnNetworkSafe<Node2D>(lootScene, "Loot");

		AddChild(item);

		item.Position = Vector2.Zero;
	}
}
