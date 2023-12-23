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

	private void OnBodyEntered(Node2D body)
	{
		if (!(body is Player)) return;

		if (_spawned) return;

		_spawned = true;

		foreach (Node2D spawnPoint in SpawnPoints)
		{
			Rpc(nameof(SpawnEnemyRpc), spawnPoint.Position);
		}
	}

	[Rpc(CallLocal = true)]
	private void SpawnEnemyRpc(Vector2 position)
	{
		Node2D enemy = EnemyScenes[0].Instantiate<Node2D>();

		enemy.Position = position;

		AddChild(enemy);
	}
}
