using System;
using System.Collections.Generic;
using Godot;
using Networking;
using Riptide;

public partial class FlockScared : EnemyState {
    public float Speed = 70f;
    public float InverseInertia = 3f;
    public float FlockOffsetRange = 24f;
    public Vector2 ScaredInterval = new Vector2(1f, 2f);
    public string AttackState = "aggressive";

    public Func<List<Enemy>> GetFlock;
    public Action OnEnter;


    private Vector2 _flockOffset;
    private RandomNumberGenerator _randomNumberGenerator = new RandomNumberGenerator();
    private float _scaredTimer = 0;
    private Vector2 _smoothedVelocity;

    public FlockScared(string name, Enemy enemy) : base(name, enemy) { }


    public override void Initialize() {
        _enemy.NetworkPoint.Register(nameof(AttackRpc), AttackRpc);
    }

    public override void Enter() {
        _smoothedVelocity = _enemy.Velocity - _enemy.Knockback;

        _scaredTimer = _randomNumberGenerator.RandfRange(ScaredInterval.X, ScaredInterval.Y);

        PickFlockOffset();

        OnEnter();
    }

    public override void Update(float delta) {
        if (_enemy.Hurt) {
            _enemy.AnimationPlayer.Stop();
        } else {
            _enemy.AnimationPlayer.Play("scared");
        }
    }

    public override void PhsysicsUpdate(float delta) {
        if (!_enemy.NetworkPoint.IsOwner) return;

        if (!_enemy.Activated) return;

        Vector2 target = Vector2.Zero;

        foreach (Enemy enemy in GetFlock()) {
            target += enemy.GlobalPosition;
        }

        target /= GetFlock().Count;

        target += _flockOffset;

        _smoothedVelocity = _smoothedVelocity.Slerp((target - _enemy.GlobalPosition).Normalized() * Speed, InverseInertia * delta);
        _enemy.Velocity = _smoothedVelocity + _enemy.Knockback;

        _enemy.Face(target);

        _enemy.MoveAndSlide();

        if (_enemy.GlobalPosition.DistanceTo(target) <= 1) PickFlockOffset();

        _scaredTimer -= delta;

        if (_scaredTimer <= 0) _enemy.NetworkPoint.BounceRpcToClients(nameof(AttackRpc));
    }

    public override void Exit() {
        _enemy.AnimationPlayer.Stop();
    }

    private void PickFlockOffset() {
        _flockOffset = (Vector2.Right * _randomNumberGenerator.RandfRange(0, FlockOffsetRange)).Rotated(_randomNumberGenerator.RandfRange(0, Mathf.Pi * 2f));
    }

    private void AttackRpc(Message message) {
        GoToState(AttackState);
    }
}
