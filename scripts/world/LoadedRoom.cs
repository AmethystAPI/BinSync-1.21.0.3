using System;
using System.Collections.Generic;
using Godot;

public class LoadableRoom {
    public WorldGenerator.RoomPlacement RoomPlacement;
    public string Id;
    public bool Cleared = false;

    private World _world;
    private Biome _biome;
    private bool _activated = false;
    private List<List<int>> _rounds = new List<List<int>>();
    private float _pointsPerRound;
    private Node2D _room;
    private List<Node2D> _barriers = new List<Node2D>();
    private int _spawnedEnemies = 0;

    public LoadableRoom(WorldGenerator.RoomPlacement roomPlacement, World world, Biome biome) {
        Id = Guid.NewGuid().ToString();

        RoomPlacement = roomPlacement;
        _world = world;
        _biome = biome;
    }

    public void Load() {
        foreach (Vector2 tileLocation in RoomPlacement.RoomLayout.Walls) {
            Vector2I realTileLocation = RoomPlacement.Location + new Vector2I((int)tileLocation.X, (int)tileLocation.Y);

            _world.WallsTileMapLayer.SetCell(realTileLocation, 0, new Vector2I(3, 0));
        }

        _room = new Node2D();
        _world.AddChild(_room);
        _room.GlobalPosition = RoomPlacement.Location * 16;

        if (RoomPlacement.Type == WorldGenerator.RoomPlacement.RoomType.Spawn) return;

        PackedScene barrierScene = ResourceLoader.Load<PackedScene>("res://scenes/rooms/barrier.tscn");

        if (Cleared) return;

        foreach (RoomLayout.Connection connection in RoomPlacement.RoomLayout.GetConnections()) {
            if (connection.Equals(RoomPlacement.EntranceConnection)) continue;

            Node2D barrier = barrierScene.Instantiate<Node2D>();
            _barriers.Add(barrier);

            _room.AddChild(barrier);

            RectangleShape2D shape = barrier.GetNode<CollisionShape2D>("CollisionShape2D").Shape as RectangleShape2D;

            if (connection.Direction == Vector2.Right || connection.Direction == Vector2.Left) {
                shape.Size = new Vector2(32, 16 * 8);
            } else {
                shape.Size = new Vector2(16 * 8, 32);
            }

            barrier.Position = connection.Location * 16;
        }
    }

    public void Unload() {
        _room.QueueFree();

        foreach (Vector2 tileLocation in RoomPlacement.RoomLayout.Walls) {
            Vector2I realTileLocation = RoomPlacement.Location + new Vector2I((int)tileLocation.X, (int)tileLocation.Y);

            _world.WallsTileMapLayer.SetCell(realTileLocation, -1);
        }

        _activated = false;
        _rounds = new List<List<int>>();
        _barriers = new List<Node2D>();
        _spawnedEnemies = 0;
    }

    public void Update() {
        if (RoomPlacement.Type != WorldGenerator.RoomPlacement.RoomType.None) return;

        if (_activated) return;

        foreach (Player player in Player.AlivePlayers) {
            if (player.GlobalPosition.X < RoomPlacement.GetTopLeftBound().X * 16) continue;
            if (player.GlobalPosition.X > RoomPlacement.GetBottomRightBound().X * 16) continue;
            if (player.GlobalPosition.Y < RoomPlacement.GetTopLeftBound().Y * 16) continue;
            if (player.GlobalPosition.Y > RoomPlacement.GetBottomRightBound().Y * 16) continue;

            Activate();

            break;
        }
    }

    public void AddEnemy(Enemy enemy) {
        enemy.OnDied += () => RemoveEnemy(enemy);
        enemy.OnSummonedEnemy += enemy => {
            _spawnedEnemies++;

            AddEnemy(enemy);
        };
    }
    public void RemoveEnemy(Enemy enemy) {
        _spawnedEnemies--;

        if (_spawnedEnemies > 0) return;

        Complete();
    }

    private void Activate() {
        _activated = true;

        SpawnEnemies(1f);
    }

    private void Complete() {
        Cleared = true;

        foreach (Node2D barrier in _barriers) {
            barrier.QueueFree();
        }
    }

    private void SpawnEnemies(float points, bool activated = false) {
        int rounds = Mathf.FloorToInt(Mathf.Pow(points / 2f, 0.8f));
        if (rounds < 1) rounds = 1;
        _pointsPerRound = points / rounds;

        while (points > 0) {
            float pointsForThisRound = _pointsPerRound;

            List<int> round = new List<int>();

            while (pointsForThisRound > 0) {
                int selectedEnemyIndex = new RandomNumberGenerator().RandiRange(0, _biome.EnemyPool.EnemyScenes.Length - 1);

                round.Add(selectedEnemyIndex);

                points -= _biome.EnemyPool.Points[selectedEnemyIndex];
                pointsForThisRound -= _biome.EnemyPool.Points[selectedEnemyIndex];

                if (_biome.EnemyPool.Points[selectedEnemyIndex] == 0) points -= 1;
                if (_biome.EnemyPool.Points[selectedEnemyIndex] == 0) pointsForThisRound -= 1;
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

            _spawnedEnemies++;

            _world.NetworkPoint.SendRpcToClients(nameof(World.SpawnEnemyRpc), message => {
                message.AddFloat(spawnPoint.X);
                message.AddFloat(spawnPoint.Y);

                message.AddString(_biome.EnemyPool.EnemyScenes[enemyTypeIndex].ResourcePath);

                message.AddString(Id);
            });
        }
    }

    private Vector2 GetRandomPointInSpawnArea() {
        return (RoomPlacement.Location + RoomPlacement.RoomLayout.SpawnLocations[Game.RandomNumberGenerator.RandiRange(0, RoomPlacement.RoomLayout.SpawnLocations.Length - 1)]) * 16f;
    }

    private Vector2 GetRandomPointAroundPlayer() {
        Player player = Player.AlivePlayers[Game.RandomNumberGenerator.RandiRange(0, Player.AlivePlayers.Count - 1)];

        return player.GlobalPosition + Vector2.Right * Game.RandomNumberGenerator.RandfRange(-64, 64) + Vector2.Up * Game.RandomNumberGenerator.RandfRange(-64, 64);
    }
}