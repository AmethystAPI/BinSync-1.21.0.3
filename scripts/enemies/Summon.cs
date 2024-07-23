using Godot;
using Networking;
using Riptide;

public partial class Summon : NodeState, NetworkPointUser {
    [Export] public PackedScene[] Summons = new PackedScene[0];
    [Export] public Vector2I SummonAmmount = new Vector2I(3, 4);

    public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

    private Enemy _enemy;
    private RandomNumberGenerator _randomNumberGenerator = new RandomNumberGenerator();

    public override void _Ready() {
        _enemy = GetParent().GetParent<Enemy>();

        NetworkPoint.Setup(this);

        NetworkPoint.Register(nameof(SummonRpc), SummonRpc);
    }

    public override void Enter() {
        if (NetworkManager.IsHost && _enemy.Activated) {
            int amount = _randomNumberGenerator.RandiRange(SummonAmmount.X, SummonAmmount.Y);

            for (int i = 0; i < amount; i++) {
                NetworkPoint.SendRpcToClientsFast(nameof(SummonRpc), message => {
                    message.AddString(Summons[_randomNumberGenerator.RandiRange(0, Summons.Length - 1)].ResourcePath);

                    message.AddFloat(_randomNumberGenerator.RandfRange(-8f, 8f));
                    message.AddFloat(_randomNumberGenerator.RandfRange(-8f, 8f));
                });
            }
        }

        GoToState("Idle");
    }

    private void SummonRpc(Message message) {
        string path = message.GetString();

        PackedScene scene = ResourceLoader.Load<PackedScene>(path);

        Enemy enemy = NetworkManager.SpawnNetworkSafe<Enemy>(scene, "Summon");

        _enemy.GetParent().AddChild(enemy);

        enemy.GlobalPosition = _enemy.GlobalPosition + new Vector2(message.GetFloat(), message.GetFloat());

        enemy.Activate();
    }
}
