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
    private float _nextRoundTimer;
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
        SmartTile wallTile = _biome.Tileset.GetTile("walls");
        SmartTile roofTile = _biome.Tileset.GetTile("roofs");
        SmartTile shadowTile = _biome.Tileset.GetTile("shadows");
        SmartTile floorTile = _biome.Tileset.GetTile("floors");
        SmartTile grassTile = _biome.Tileset.GetTile("grass");

        _biome.Tileset.Apply(_world.WallsTileMapLayer);
        _biome.Tileset.Apply(_world.RoofsTileMapLayer);
        _biome.Tileset.Apply(_world.FloorsTileMapLayer);
        _biome.Tileset.Apply(_world.ShadowsTileMapLayer);
        _biome.Tileset.Apply(_world.GrassTileMapLayer);

        FastNoiseLite noise = new FastNoiseLite();
        noise.Seed = (int)Game.Seed;
        noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
        noise.Frequency = 0.06f;
        noise.FractalLacunarity = 2;
        noise.FractalGain = 0.7f;

        foreach (Vector2 tileLocation in RoomPlacement.RoomLayout.Walls) {
            Vector2I realTileLocation = RoomPlacement.Location + new Vector2I((int)tileLocation.X, (int)tileLocation.Y);

            SmartTile.Tile? possibleWallTile = wallTile.GetTile(realTileLocation, RoomPlacement.IsTileWallOrBounds);
            SmartTile.Tile? possibleRoofTile = roofTile.GetTile(realTileLocation, RoomPlacement.IsTileWallOrBounds);
            SmartTile.Tile? possibleShadowTile = shadowTile.GetTile(realTileLocation + Vector2I.Down, RoomPlacement.IsTileWallOrBounds);
            SmartTile.Tile? possibleUpperShadowTile = shadowTile.GetTile(realTileLocation, RoomPlacement.IsTileWallOrBounds);


            if (possibleWallTile is SmartTile.Tile wallTileData) _world.WallsTileMapLayer.SetCell(realTileLocation, wallTileData.Source, wallTileData.Location);
            if (possibleRoofTile is SmartTile.Tile roofTileData) _world.RoofsTileMapLayer.SetCell(realTileLocation, roofTileData.Source, roofTileData.Location);
            if (possibleShadowTile is SmartTile.Tile shadowTileData) _world.ShadowsTileMapLayer.SetCell(realTileLocation + Vector2I.Down, shadowTileData.Source, shadowTileData.Location);
            if (possibleUpperShadowTile is SmartTile.Tile upperShadowTileData) _world.ShadowsTileMapLayer.SetCell(realTileLocation, upperShadowTileData.Source, upperShadowTileData.Location);
        }

        for (int x = (int)RoomPlacement.GetTopLeftBound().X; x < (int)RoomPlacement.GetBottomRightBound().X; x++) {
            for (int y = (int)RoomPlacement.GetTopLeftBound().Y; y < (int)RoomPlacement.GetBottomRightBound().Y; y++) {
                SmartTile.Tile? possibleFloorTile = floorTile.GetTile(new Vector2I(x, y), RoomPlacement.IsTileWallOrBounds);
                SmartTile.Tile? possibleGrassTile = grassTile.GetTile(new Vector2I(x, y), location => {
                    return noise.GetNoise2D(location.X, location.Y) > 0;
                });

                if (possibleFloorTile is SmartTile.Tile floorTileData) _world.FloorsTileMapLayer.SetCell(new Vector2I(x, y), floorTileData.Source, floorTileData.Location);
                if (possibleGrassTile is SmartTile.Tile grassTileData) _world.GrassTileMapLayer.SetCell(new Vector2I(x, y), grassTileData.Source, grassTileData.Location);
            }
        }

        _room = new Node2D();
        _room.YSortEnabled = true;

        _world.AddChild(_room);
        _room.GlobalPosition = RoomPlacement.Location * 16;

        foreach (WorldGenerator.DecorationPlacement decorationPlacement in RoomPlacement.Decorations) {
            Node2D decoration = decorationPlacement.Scene.Instantiate<Node2D>();

            _room.AddChild(decoration);

            decoration.GlobalPosition = decorationPlacement.Location * 16 + Vector2.One * 8f;
        }

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
            _world.RoofsTileMapLayer.SetCell(realTileLocation, -1);
            _world.ShadowsTileMapLayer.SetCell(realTileLocation + Vector2I.Down, -1);
            _world.ShadowsTileMapLayer.SetCell(realTileLocation, -1);
        }

        for (int x = (int)RoomPlacement.GetTopLeftBound().X; x < (int)RoomPlacement.GetBottomRightBound().X; x++) {
            for (int y = (int)RoomPlacement.GetTopLeftBound().Y; y < (int)RoomPlacement.GetBottomRightBound().Y; y++) {
                _world.FloorsTileMapLayer.SetCell(new Vector2I(x, y), -1);
            }
        }

        _activated = false;
        _rounds = new List<List<int>>();
        _barriers = new List<Node2D>();
        _spawnedEnemies = 0;
    }

    public void Update(float delta) {
        if (RoomPlacement.Type != WorldGenerator.RoomPlacement.RoomType.None) return;

        if (!_activated) {
            foreach (Player player in Player.AlivePlayers) {
                if (player.GlobalPosition.X < RoomPlacement.GetTopLeftBound().X * 16) continue;
                if (player.GlobalPosition.X > RoomPlacement.GetBottomRightBound().X * 16) continue;
                if (player.GlobalPosition.Y < RoomPlacement.GetTopLeftBound().Y * 16) continue;
                if (player.GlobalPosition.Y > RoomPlacement.GetBottomRightBound().Y * 16) continue;

                Activate();

                break;
            }

            return;
        }

        _nextRoundTimer -= delta;

        if (_nextRoundTimer > 0) return;
        if (_rounds.Count == 0) return;

        SpawnEnemiesFromRound(_rounds[0], true, true);
        _rounds.RemoveAt(0);

        _nextRoundTimer = Mathf.Pow(_pointsPerRound * 8, 0.8f);
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
        if (_rounds.Count > 0) return;

        Complete();
    }

    private void Activate() {
        _activated = true;

        SpawnEnemies(Game.Difficulty);
    }

    private void Complete() {
        Cleared = true;

        foreach (Node2D barrier in _barriers) {
            barrier.QueueFree();
        }

        Game.IncreaseDifficulty();
    }

    private void SpawnEnemies(float points, bool activated = false) {
        int rounds = Mathf.FloorToInt(Mathf.Pow(points / 2f, 0.8f));

        if (rounds < 1) rounds = 1;
        _pointsPerRound = points / rounds;

        _nextRoundTimer = Mathf.Pow(_pointsPerRound * 8, 0.8f);

        while (points > 0) {
            float pointsForThisRound = _pointsPerRound;

            List<int> round = new List<int>();

            while (pointsForThisRound > 0) {
                int selectedEnemyIndex = new RandomNumberGenerator().RandiRange(0, _biome.EnemyPool.Entries.Length - 1);

                round.Add(selectedEnemyIndex);

                points -= _biome.EnemyPool.Entries[selectedEnemyIndex].Points;
                pointsForThisRound -= _biome.EnemyPool.Entries[selectedEnemyIndex].Points;

                if (_biome.EnemyPool.Entries[selectedEnemyIndex].Points == 0) points -= 1;
                if (_biome.EnemyPool.Entries[selectedEnemyIndex].Points == 0) pointsForThisRound -= 1;
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

                message.AddString(_biome.EnemyPool.Entries[enemyTypeIndex].Scene.ResourcePath);

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