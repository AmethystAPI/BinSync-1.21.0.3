using Godot;
using System;
using System.Linq;

public partial class Room : Node2D
{
	[Export] public PackedScene[] EnemyScenes;
	[Export] public PackedScene[] LootScenes;
	[Export] public PackedScene ItemPickupScene;
	[Export] public Node2D[] SpawnPoints;

	private bool _spawned = false;
	private bool _connectedLeft = false;
	private bool _connectedRight = false;
	private bool _connectedTop = false;
	private bool _connectedBottom = false;
	private int _aliveEnemies = 0;
	private int _playersEntered = 0;

	public override void _Ready()
	{
		if (!Game.Me.IsHost) return;

		Area2D spawnTriggerArea = GetNode<Area2D>("SpawnTriggerArea");

		spawnTriggerArea.BodyEntered += OnBodyEntered;
		spawnTriggerArea.BodyExited += OnBodyExited;
	}

	public void ConnectRooms(bool connectedLeft, bool connectedRight, bool connectedTop, bool connectedBottom)
	{
		_connectedLeft = connectedLeft;
		_connectedRight = connectedRight;
		_connectedTop = connectedTop;
		_connectedBottom = connectedBottom;

		UpdateTileMaps(!_connectedLeft, !_connectedRight, !_connectedTop, !_connectedBottom);
	}

	public void AddEnemy()
	{
		_aliveEnemies++;
	}

	public void RemoveEnemy()
	{
		_aliveEnemies--;

		if (_aliveEnemies != 0) return;

		UpdateTileMaps(!_connectedLeft, !_connectedRight, !_connectedTop, !_connectedBottom);

		if (!Game.Me.IsHost) return;

		Rpc(nameof(SpawnLootRpc), LootScenes[new RandomNumberGenerator().RandiRange(0, LootScenes.Length - 1)].ResourcePath);
	}

	[Rpc(CallLocal = true)]
	private void SpawnLootRpc(string lootScenePath)
	{
		PackedScene lootScene = ResourceLoader.Load<PackedScene>(lootScenePath);

		ItemPickup itemPickup = ItemPickupScene.Instantiate<ItemPickup>();

		AddChild(itemPickup);

		itemPickup.Position = Vector2.Zero;

		itemPickup.Item = lootScene;
	}

	private void UpdateTileMaps(bool left, bool right, bool top, bool bottom)
	{
		SetEnabledTileMap("TileMapLeft", left);
		SetEnabledTileMap("TileMapRight", right);
		SetEnabledTileMap("TileMapTop", top);
		SetEnabledTileMap("TileMapBottom", bottom);
	}

	private void SetEnabledTileMap(string name, bool enabled)
	{
		TileMap tileMap = GetNode<TileMap>(name);

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

		CallDeferred(nameof(SpawnEnemies));
	}

	private void OnBodyExited(Node2D body)
	{
		if (!(body is Player)) return;

		_playersEntered--;
	}

	private void SpawnEnemies()
	{
		if (_spawned) return;

		_spawned = true;

		Rpc(nameof(StartRoomRpc));

		foreach (Node2D spawnPoint in SpawnPoints)
		{
			Rpc(nameof(SpawnEnemyRpc), spawnPoint.GlobalPosition);
		}
	}

	[Rpc(CallLocal = true)]
	private void StartRoomRpc()
	{
		UpdateTileMaps(true, true, true, true);
	}

	[Rpc(CallLocal = true)]
	private void SpawnEnemyRpc(Vector2 position)
	{
		Node2D enemy = EnemyScenes[0].Instantiate<Node2D>();

		enemy.SetMultiplayerAuthority(1);

		AddChild(enemy);

		enemy.GlobalPosition = position;
	}
}
