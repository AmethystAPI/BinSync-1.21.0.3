using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Room : Node2D
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

	private bool _spawned = false;
	private int _aliveEnemies = 0;
	private int _playersEntered = 0;

	public override void _Ready()
	{
		foreach (Node2D entrance in Entrances)
		{
			entrance.GetParent().RemoveChild(entrance);
		}

		foreach (Node2D exit in Exits)
		{
			exit.GetParent().RemoveChild(exit);
		}

		if (!Game.Me.IsHost) return;

		Area2D spawnTriggerArea = GetNode<Area2D>("SpawnTriggerArea");

		spawnTriggerArea.BodyEntered += OnBodyEntered;
		spawnTriggerArea.BodyExited += OnBodyExited;
	}

	public virtual void PlaceEntrance(Vector2 direction)
	{
		SetTileMapEnabled(EdgeTileMaps[EdgeTileMapDirections.ToList().IndexOf(direction)], false);

		AddChild(Entrances[EntranceDirections.ToList().IndexOf(direction)]);
	}

	public virtual void PlaceExit(Vector2 direction)
	{
		SetTileMapEnabled(EdgeTileMaps[EdgeTileMapDirections.ToList().IndexOf(direction)], false);

		AddChild(Exits[ExitDirections.ToList().IndexOf(direction)]);
	}

	public void AddEnemy()
	{
		_aliveEnemies++;
	}

	public void RemoveEnemy()
	{
		_aliveEnemies--;

		if (_aliveEnemies != 0) return;

		End();
	}

	private void SetTileMapEnabled(TileMap tileMap, bool enabled)
	{
		for (int layerIndex = 0; layerIndex < tileMap.GetLayersCount(); layerIndex++)
		{
			tileMap.SetLayerEnabled(layerIndex, enabled);
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		if (!(body is Player)) return;

		_playersEntered++;

		if (_playersEntered != Player.Players.Count) return;

		Rpc(nameof(StartRpc));
	}

	private void OnBodyExited(Node2D body)
	{
		if (!(body is Player)) return;

		_playersEntered--;
	}

	[Rpc(CallLocal = true)]
	private void StartRpc()
	{
		Start();
	}

	protected virtual void Start()
	{
		Started?.Invoke();

		WorldGenerator.DespawnLastRoom();

		if (!Game.Me.IsHost) return;

		SpawnEnemies();
	}

	private void SpawnEnemies()
	{
		if (_spawned) return;

		_spawned = true;

		foreach (Node2D spawnPoint in SpawnPoints)
		{
			Rpc(nameof(SpawnEnemyRpc), spawnPoint.GlobalPosition, new RandomNumberGenerator().RandiRange(0, EnemyScenes.Length - 1));
		}
	}

	[Rpc(CallLocal = true)]
	private void SpawnEnemyRpc(Vector2 position, int enemySceneIndex)
	{
		Node2D enemy = EnemyScenes[enemySceneIndex].Instantiate<Node2D>();

		enemy.SetMultiplayerAuthority(1);

		AddChild(enemy);

		enemy.GlobalPosition = position;
	}

	protected virtual void End()
	{
		Completed?.Invoke();

		if (!Game.Me.IsHost) return;

		Rpc(nameof(SpawnLootRpc), LootScenes[new RandomNumberGenerator().RandiRange(0, LootScenes.Length - 1)].ResourcePath);
	}

	[Rpc(CallLocal = true)]
	protected void SpawnLootRpc(string lootScenePath)
	{
		PackedScene lootScene = ResourceLoader.Load<PackedScene>(lootScenePath);

		Node2D item = lootScene.Instantiate<Node2D>();

		AddChild(item);

		item.Position = Vector2.Zero;
	}
}
