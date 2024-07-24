using System.Linq;
using Godot;
using Networking;
using Riptide;

public class RandomIdle : EnemyState {
    public Vector2 IdleInterval = new Vector2(0.8f, 1.2f);
    public string[] AttackStates = new string[] { "attack" };
    public float[] AttackWeights = new float[] { 1f };

    private float _idleTimer = 0;
    private RandomNumberGenerator _randomNumberGenerator = new RandomNumberGenerator();
    public RandomIdle(string name, Enemy enemy) : base(name, enemy) { }


    public override void Initialize() {
        _enemy.NetworkPoint.Register(nameof(AttackRpc), AttackRpc);
    }

    public override void Enter() {
        _enemy.AnimationPlayer.Play("idle");

        _idleTimer = _randomNumberGenerator.RandfRange(IdleInterval.X, IdleInterval.Y);
    }

    public override void Update(float delta) {
        if (!NetworkManager.IsHost) return;

        if (!_enemy.Activated) return;

        _idleTimer -= delta;

        if (_idleTimer > 0) return;

        float totalWeights = AttackWeights.Sum();
        float selection = _randomNumberGenerator.RandfRange(0f, totalWeights - 0.0001f);

        string state = AttackStates[0];

        for (int index = 0; index < AttackStates.Length; index++) {
            if (selection < AttackWeights[index]) {
                state = AttackStates[index];

                break;
            }

            selection -= AttackWeights[index];
        }

        _enemy.NetworkPoint.SendRpcToClientsFast(nameof(AttackRpc), message => message.AddString(state));
    }

    public override void PhsysicsUpdate(float delta) {
        _enemy.Velocity = _enemy.Knockback;

        _enemy.MoveAndSlide();
    }

    public override void Exit() {
        _enemy.AnimationPlayer.Stop();
    }

    private void AttackRpc(Message message) {
        GoToState(message.GetString());
    }
}
