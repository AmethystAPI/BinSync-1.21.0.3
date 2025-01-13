using System;
using System.Collections.Generic;
using Godot;

public class LoadedRoom {
    public WorldGenerator.RoomPlacement RoomPlacement;
    public string Id;

    private World _world;
    private Biome _biome;
    private bool _activated = false;
    private List<List<int>> _rounds = new List<List<int>>();
    private float _pointsPerRound;

    public LoadedRoom(WorldGenerator.RoomPlacement roomPlacement, World world, Biome biome) {
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
    }

    public void Unload() {
        foreach (Vector2 tileLocation in RoomPlacement.RoomLayout.Walls) {
            Vector2I realTileLocation = RoomPlacement.Location + new Vector2I((int)tileLocation.X, (int)tileLocation.Y);

            _world.WallsTileMapLayer.SetCell(realTileLocation, -1);
        }
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

    private void Activate() {
        _activated = true;

        GD.Print("Room activated! " + RoomPlacement.Location);

        SpawnEnemies(4f);
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

            // while (DetectSpawnOverlap(spawnPoint).Count > 0) {
            //     spawnPoint = spawnAroundPlayers ? GetRandomPointAroundPlayer() : GetRandomPointInSpawnArea();
            // }

            _world.NetworkPoint.SendRpcToClients(nameof(World.SpawnEnemyRpc), message => {
                message.AddFloat(spawnPoint.X);
                message.AddFloat(spawnPoint.Y);

                message.AddString(_biome.EnemyPool.EnemyScenes[enemyTypeIndex].ResourcePath);

                message.AddString(Id);
            });
        }
    }

    private Vector2 GetRandomPointInSpawnArea() {
        Vector2 position = RoomPlacement.Location * 16;

        // RectangleShape2D shape = _spawnArea.Shape as RectangleShape2D;

        // position += Vector2.Right * Game.RandomNumberGenerator.RandfRange(-shape.Size.X / 2, shape.Size.X / 2) + Vector2.Up * Game.RandomNumberGenerator.RandfRange(-shape.Size.Y / 2, shape.Size.Y / 2);

        return position;
    }

    private Vector2 GetRandomPointAroundPlayer() {
        Player player = Player.AlivePlayers[Game.RandomNumberGenerator.RandiRange(0, Player.AlivePlayers.Count - 1)];

        return player.GlobalPosition + Vector2.Right * Game.RandomNumberGenerator.RandfRange(-64, 64) + Vector2.Up * Game.RandomNumberGenerator.RandfRange(-64, 64);
    }
}