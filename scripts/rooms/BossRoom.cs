using Godot;
using Networking;
using Riptide;
using System;

public partial class BossRoom : Room {
    [Export] public PackedScene BossScene;
    [Export] public Node2D SpawnPoint;

    public override void _Ready() {
        base._Ready();

        NetworkPoint.Register(nameof(SpawnBossRpc), SpawnBossRpc);
    }

    internal override void SpawnComponents() {
        NetworkPoint.SendRpcToClients(nameof(SpawnBossRpc));
    }

    private void SpawnBossRpc(Message message) {
        Enemy enemy = NetworkManager.SpawnNetworkSafe<Enemy>(BossScene, "Boss");

        AddChild(enemy);

        enemy.GlobalPosition = SpawnPoint.GlobalPosition;

        _spawnedEnemies.Add(enemy);

        GD.Print(BossScene);
    }
}
