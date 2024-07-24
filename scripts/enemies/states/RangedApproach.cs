using Godot;
using Networking;
using Riptide;

public partial class RangedApproach
 : EnemyState {
    public Vector2 IdleInterval = new Vector2(1.5f, 2.5f);
    public float Speed = 30;
    public float TargetDistance = 64;
    public float RestRange = 8;
    public string AttackState = "attack";

    private float _idleTimer = 0;
    private RandomNumberGenerator _randomNumberGenerator = new RandomNumberGenerator();
    private bool _resting = false;

    public RangedApproach(string name, Enemy enemy) : base(name, enemy) { }

    public override void Initialize() {
        _enemy.NetworkPoint.Register(nameof(AttackRpc), AttackRpc);
    }

    public override void Enter() {
        _idleTimer = _randomNumberGenerator.RandfRange(IdleInterval.X, IdleInterval.Y);
    }

    public override void Update(float delta) {
        if (!NetworkManager.IsHost) return;

        if (!_enemy.Activated) return;

        _idleTimer -= delta;

        if (_idleTimer > 0) return;

        _enemy.NetworkPoint.SendRpcToClientsFast(nameof(AttackRpc));
    }

    public override void PhsysicsUpdate(float delta) {
        if (!_enemy.Activated) return;

        Enemy.WeightedTarget[] targets = _enemy.GetWeightedTargets();

        if (targets.Length == 0) return;

        Vector2 target = targets[0].Player.GlobalPosition;
        float distance = target.DistanceTo(_enemy.GlobalPosition);
        Vector2 direction = (target - _enemy.GlobalPosition).Normalized();

        _enemy.Face(target);

        if (_enemy.Hurt) _enemy.AnimationPlayer.Stop();

        if (_resting) {
            if (Mathf.Abs(distance - TargetDistance) <= RestRange) {
                _enemy.Velocity = _enemy.Knockback;

                _enemy.MoveAndSlide();

                if (!_enemy.Hurt) _enemy.AnimationPlayer.Play("idle");

                return;
            }

            _resting = false;
        }

        if (Mathf.Abs(distance - TargetDistance) <= 1f) {
            _resting = true;

            _enemy.Velocity = _enemy.Knockback;

            _enemy.MoveAndSlide();

            if (!_enemy.Hurt) _enemy.AnimationPlayer.Play("idle");

            return;
        }

        _enemy.Velocity = direction * Speed * (distance > TargetDistance ? 1 : -1) + _enemy.Knockback;

        if (!_enemy.Hurt) _enemy.AnimationPlayer.Play("run");

        _enemy.MoveAndSlide();
    }

    public override void Exit() {
        _enemy.AnimationPlayer.Stop();
    }

    private void AttackRpc(Message message) {
        GoToState(AttackState);
    }
}
