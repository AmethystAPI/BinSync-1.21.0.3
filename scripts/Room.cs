using Godot;
using Riptide;
using System;
using System.Linq;

public partial class Room : Node2D, Networking.NetworkNode
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

	private Networking.RpcMap _rpcMap = new Networking.RpcMap();
	public Networking.RpcMap RpcMap => _rpcMap;

	private bool _spawned = false;
	private int _aliveEnemies = 0;
	private int _playersEntered = 0;
	private bool _started = false;

	public override void _Ready()
	{
		_rpcMap.Register(nameof(StartRpc), StartRpc);
		_rpcMap.Register(nameof(EndRpc), EndRpc);
		_rpcMap.Register(nameof(SpawnEnemyRpc), SpawnEnemyRpc);
		_rpcMap.Register(nameof(SpawnLootRpc), SpawnLootRpc);

		foreach (Node2D entrance in Entrances)
		{
			entrance.GetParent().RemoveChild(entrance);
		}

		foreach (Node2D exit in Exits)
		{
			exit.GetParent().RemoveChild(exit);
		}

		if (!Game.IsHost()) return;

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

		if (!Game.IsHost()) return;

		Game.SendRpcToClients(this, nameof(EndRpc), MessageSendMode.Reliable, message => { });
	}

	private void OnBodyEntered(Node2D body)
	{
		if (!(body is Player)) return;

		_playersEntered++;

		if (_playersEntered != Player.Players.Count) return;

		if (_started) return;

		_started = true;

		Game.SendRpcToClients(this, nameof(StartRpc), MessageSendMode.Reliable, message => { });
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

		if (!Game.IsHost()) return;

		SpawnEnemies();
	}

	private void SpawnEnemies()
	{
		if (_spawned) return;

		_spawned = true;

		foreach (Node2D spawnPoint in SpawnPoints)
		{
			Game.SendRpcToClients(this, nameof(SpawnEnemyRpc), MessageSendMode.Reliable, message =>
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

		try
		{
			Node2D enemy = EnemyScenes[enemySceneIndex].Instantiate<Node2D>();
			Game.NameSpawnedNetworkNode("Enemy", enemy);

			enemy.SetMultiplayerAuthority(1);

			AddChild(enemy);

			enemy.GlobalPosition = position;
		}
		catch
		{
			GD.PushError("HUH? " + enemySceneIndex);
		}
	}

	protected virtual void EndRpc(Message message)
	{
		Completed?.Invoke();

		if (!Game.IsHost()) return;

		if (LootScenes.Length == 0) return;

		Game.SendRpcToClients(this, nameof(SpawnLootRpc), MessageSendMode.Reliable, message =>
		{
			message.AddString(LootScenes[new RandomNumberGenerator().RandiRange(0, LootScenes.Length - 1)].ResourcePath);
		});
	}

	protected void SpawnLootRpc(Message message)
	{
		string lootScenePath = message.GetString();

		PackedScene lootScene = ResourceLoader.Load<PackedScene>(lootScenePath);

		Node2D item = lootScene.Instantiate<Node2D>();
		Game.NameSpawnedNetworkNode("Loot", item);

		AddChild(item);

		item.Position = Vector2.Zero;
	}
}
