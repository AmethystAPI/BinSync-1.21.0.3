using System.Linq;
using Godot;
using Networking;
using Riptide;

public partial class RandomIdle : NodeState, NetworkPointUser {
    [Export] public Vector2 IdleInterval = new Vector2(0.8f, 1.2f);
    [Export] public string[] AttackStates = new string[] { "Attack" };
    [Export] public float[] AttackWeights = new float[] { 1f };
    [Export] public string Animation = "idle";
    [Export] public AnimationPlayer AnimationPlayer;

    public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

    private Enemy _enemy;
    private float _idleTimer = 0;
    private RandomNumberGenerator _randomNumberGenerator = new RandomNumberGenerator();
    private float _lastIdleTime;

    public override void _Ready() {
        _enemy = GetParent().GetParent<Enemy>();

        NetworkPoint.Setup(this);

        NetworkPoint.Register(nameof(AttackRpc), AttackRpc);
    }

    public override void Enter() {
        AnimationPlayer.Play(Animation);

        if (_idleTimer > 0) {
            _idleTimer -= (Time.GetTicksMsec() - _lastIdleTime) / 100f;

            return;
        }

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

        NetworkPoint.SendRpcToClientsFast(nameof(AttackRpc), message => message.AddString(state));
    }

    public override void Exit() {
        _lastIdleTime = Time.GetTicksMsec();
    }

    private void AttackRpc(Message message) {
        GoToState(message.GetString());
    }
}
