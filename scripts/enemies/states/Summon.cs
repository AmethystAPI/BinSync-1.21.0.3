using System;
using Godot;
using Networking;
using Riptide;

public partial class Summon : EnemyState {
    public PackedScene[] Summons = new PackedScene[0];
    public Vector2I SummonAmmount = new Vector2I(3, 4);
    public string ReturnState = "idle";

    public Action OnSummon;

    private RandomNumberGenerator _randomNumberGenerator = new RandomNumberGenerator();

    public Summon(string name, Enemy enemy) : base(name, enemy) {
    }

    public override void Initialize() {
        _enemy.NetworkPoint.Register(nameof(SummonRpc), SummonRpc);
    }

    public override void Enter() {
        if (NetworkManager.IsHost && _enemy.Activated) {
            int amount = _randomNumberGenerator.RandiRange(SummonAmmount.X, SummonAmmount.Y);

            for (int i = 0; i < amount; i++) {
                Room.Current.AddEnemy();

                _enemy.NetworkPoint.SendRpcToClientsFast(nameof(SummonRpc), message => {
                    message.AddString(Summons[_randomNumberGenerator.RandiRange(0, Summons.Length - 1)].ResourcePath);

                    message.AddFloat(_randomNumberGenerator.RandfRange(-8f, 8f));
                    message.AddFloat(_randomNumberGenerator.RandfRange(-8f, 8f));
                });
            }
        }

        GoToState(ReturnState);
    }

    public override void PhsysicsUpdate(float delta) {
        _enemy.Velocity = _enemy.Knockback;

        _enemy.MoveAndSlide();
    }

    private void SummonRpc(Message message) {
        string path = message.GetString();

        PackedScene scene = ResourceLoader.Load<PackedScene>(path);

        Enemy enemy = NetworkManager.SpawnNetworkSafe<Enemy>(scene, "Summon");

        _enemy.GetParent().AddChild(enemy);

        enemy.GlobalPosition = _enemy.GlobalPosition + new Vector2(message.GetFloat(), message.GetFloat());

        enemy.Activate();

        OnSummon();
    }
}
