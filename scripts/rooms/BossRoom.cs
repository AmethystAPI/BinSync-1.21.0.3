using Godot;
using Networking;
using Riptide;

public partial class BossRoom : Room {
    [Export] public PackedScene BossScene;
    [Export] public Node2D SpawnPoint;
    [Export] public LootPool LootPool;

    public override void _Ready() {
        base._Ready();

        NetworkPoint.Register(nameof(SpawnBossRpc), SpawnBossRpc);
        NetworkPoint.Register(nameof(SpawnLootRpc), SpawnLootRpc);
    }

    protected override void SpawnComponents() {
        NetworkPoint.SendRpcToClients(nameof(SpawnBossRpc));
    }

    protected override void ActivateEnemiesRpc(Message message) {
        Audio.PlayMusic("boss");
    }

    protected override void Complete(Enemy enemy = null) {
        base.Complete(enemy);

        Audio.PlayMusic("golden_grove");

        if (!NetworkManager.IsHost) return;

        foreach (Connection client in NetworkManager.LocalServer.Clients) {
            NetworkPoint.SendRpcToClients(nameof(SpawnLootRpc), message => {
                message.AddInt(Game.RandomNumberGenerator.RandiRange(0, LootPool.LootScenes.Length - 1));
                message.AddFloat(enemy.GlobalPosition.X + new RandomNumberGenerator().RandfRange(-8f, 8f));
                message.AddFloat(enemy.GlobalPosition.Y + new RandomNumberGenerator().RandfRange(-8f, 8f));
            });
        }
    }

    private void SpawnBossRpc(Message message) {
        Enemy enemy = NetworkManager.SpawnNetworkSafe<Enemy>(BossScene, "Boss");

        AddChild(enemy);

        enemy.GlobalPosition = SpawnPoint.GlobalPosition;

        _spawnedEnemies.Add(enemy);
    }

    private void SpawnLootRpc(Message message) {
        int lootIndex = message.GetInt();
        Vector2 position = new Vector2(message.GetFloat(), message.GetFloat());

        Weapon item = NetworkManager.SpawnNetworkSafe<Weapon>(LootPool.LootScenes[lootIndex], "Loot");
        AddChild(item);

        item.GlobalPosition = position;
    }
}
