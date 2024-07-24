using System;
using Godot;
using Riptide;

public class FlyingAggressive : EnemyState {
    public float Speed = 70f;
    public float InverseInertia = 2f;
    public string ReturnState = "scared";

    public Func<Projectile> OnEnter;

    private Projectile _projectile;
    private Vector2 _smoothedVelocity;

    public FlyingAggressive(string name, Enemy enemy) : base(name, enemy) { }

    public override void Initialize() {
        _enemy.NetworkPoint.Register(nameof(ScaredRpc), ScaredRpc);
    }

    public override void Enter() {
        _smoothedVelocity = _enemy.Velocity - _enemy.Knockback;

        _projectile = OnEnter();

        _projectile.Destroyed += () => _enemy.NetworkPoint.BounceRpcToClients(nameof(ScaredRpc));
    }

    public override void Update(float delta) {
        if (_enemy.Hurt) {
            _enemy.AnimationPlayer.Stop();
        } else {
            _enemy.AnimationPlayer.Play("aggressive");
        }
    }

    public override void PhsysicsUpdate(float delta) {
        if (Player.AlivePlayers.Count == 0) {
            GoToState("scared");

            return;
        }

        if (!_enemy.NetworkPoint.IsOwner) return;

        Vector2 target = _enemy.GetWeightedTargets()[0].Player.GlobalPosition;

        _smoothedVelocity = _smoothedVelocity.Slerp((target - _enemy.GlobalPosition).Normalized() * Speed, InverseInertia * delta);
        _enemy.Velocity = _smoothedVelocity + _enemy.Knockback;

        _enemy.Face(target);

        _enemy.MoveAndSlide();
    }

    public override void Exit() {
        if (GodotObject.IsInstanceValid(_projectile)) _projectile.QueueFree();

        _enemy.AnimationPlayer.Stop();
    }

    private void ScaredRpc(Message message) {
        GoToState(ReturnState);
    }
}
