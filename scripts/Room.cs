using Godot;
using System;

public partial class Room : Node2D
{
	[Export] public PackedScene[] EnemyScenes;
	[Export] public Node2D[] SpawnPoints;

	private bool _spawned = false;

	public override void _Ready()
	{
		if (!Game.Me.IsHost) return;

		Area2D spawnTriggerArea = GetNode<Area2D>("SpawnTriggerArea");

		spawnTriggerArea.BodyEntered += OnBodyEntered;
	}

	public void ConnectRooms(bool connectedLeft, bool connectedRight, bool connectedTop, bool connectedBottom)
	{
		if (!connectedLeft) EnableTileMap("TileMapLeft");
		if (!connectedRight) EnableTileMap("TileMapRight");
		if (!connectedTop) EnableTileMap("TileMapTop");
		if (!connectedBottom) EnableTileMap("TileMapBottom");
	}

	private void EnableTileMap(string name)
	{
		TileMap tileMap = GetNode<TileMap>(name);

		for (int layerIndex = 0; layerIndex < tileMap.GetLayersCount(); layerIndex++)
		{
			tileMap.SetLayerEnabled(layerIndex, true);
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		if (!(body is Player)) return;

		CallDeferred(nameof(SpawnEnemies));
	}

	private void SpawnEnemies()
	{
		if (_spawned) return;

		_spawned = true;

		foreach (Node2D spawnPoint in SpawnPoints)
		{
			Rpc(nameof(SpawnEnemyRpc), spawnPoint.GlobalPosition);
		}
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
