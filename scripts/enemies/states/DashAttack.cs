using System;
using Godot;

public class DashAttack : EnemyState {
    public float Speed = 75f;
    public string ReturnState = "idle";
    public float Variance = Mathf.Pi / 6f;

    public Func<Vector2, Vector2, Projectile> OnDash;
    public Action OnStop;

    public RandomNumberGenerator Random = new RandomNumberGenerator();

    private Vector2 _direction;
    private Projectile _projectile;

    public DashAttack(string name, Enemy enemy) : base(name, enemy) { }

    public override void Enter() {
        if (Player.AlivePlayers.Count == 0) {
            GoToState("idle");

            return;
        }

        Vector2 target = _enemy.GetWeightedTargets()[0].Player.GlobalPosition;

        foreach (Player player in Player.AlivePlayers) {
            if (_enemy.GlobalPosition.DistanceTo(player.GlobalPosition) >= _enemy.GlobalPosition.DistanceTo(target)) continue;

            target = player.GlobalPosition;
        }

        _direction = (target - _enemy.GlobalPosition).Normalized();

        _direction = _direction.Rotated(Random.RandfRange(-Variance, Variance));

        _projectile = OnDash(_direction, target);

        if (_enemy.Hurt) {
            _enemy.AnimationPlayer.Stop();
        } else {
            if (target.Y > _enemy.GlobalPosition.Y) {
                _enemy.AnimationPlayer.Play("dash");
            } else {
                _enemy.AnimationPlayer.Play("dash_back");
            }
        }

        _projectile.Destroyed += () => GoToState(ReturnState);
    }

    public override void PhsysicsUpdate(float delta) {
        _enemy.Velocity = _direction * Speed + _enemy.Knockback;

        _enemy.MoveAndSlide();
    }

    public override void Exit() {
        OnStop();

        _enemy.AnimationPlayer.Stop();

        if (GodotObject.IsInstanceValid(_projectile)) _projectile.QueueFree();
    }
}
