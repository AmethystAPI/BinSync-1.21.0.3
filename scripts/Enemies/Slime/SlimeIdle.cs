using Godot;
using Networking;
using Riptide;

public partial class SlimeIdle : State, NetworkPointUser
{
    public Vector2 IdleInterval = new Vector2(0.8f, 1.2f);
    public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

    private Slime _slime;
    private float _idleTimer = 0;
    private RandomNumberGenerator _randomNumberGenerator = new RandomNumberGenerator();

    public override void _Ready()
    {
        _slime = GetParent().GetParent<Slime>();

        NetworkPoint.Setup(this);

        NetworkPoint.Register(nameof(AttackRpc), AttackRpc);
    }

    public override void Enter()
    {
        if (_idleTimer > 0) return;

        _idleTimer = _randomNumberGenerator.RandfRange(IdleInterval.X, IdleInterval.Y);
    }

    public override void Update(float delta)
    {
        if (!NetworkManager.IsHost) return;

        _idleTimer -= delta;

        if (_idleTimer > 0) return;

        NetworkPoint.SendRpcToClients(nameof(AttackRpc));
    }

    private void AttackRpc(Message message)
    {
        GoToState("Jump");
    }
}
