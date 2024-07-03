using System.Collections.Generic;
using Godot;
using Networking;
using Riptide;

public partial class CactusSpiritAttack : State {
    [Export] public PackedScene ProjectileScene;

    private Enemy _enemy;
    private float _timer = 0.5f;
    private List<float> _shootQueue = new List<float>();
    private AnimationPlayer _animationPlayer;

    public override void _Ready() {
        _enemy = GetParent().GetParent<Enemy>();
        _animationPlayer = GetParent().GetParent().GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void Enter() {
        Shoot();

        _shootQueue.Add(0.1f);
        _shootQueue.Add(0.2f);

        _timer = 0.5f;
    }

    public override void Update(float delta) {
        for (int index = 0; index < _shootQueue.Count; index++) {
            _shootQueue[index] -= (float)delta;

            if (_shootQueue[index] > 0) continue;

            Shoot();

            _shootQueue.RemoveAt(index);

            index--;
        }

        _timer -= delta;

        if (_timer <= 0f) GoToState("RangedApproach");
    }

    private void Shoot() {
        Projectile _projectile = ProjectileScene.Instantiate<Projectile>();

        _projectile.Source = _enemy;

        _enemy.GetParent().AddChild(_projectile);

        Vector2 target = _enemy.GetWeightedTargets()[0].Player.GlobalPosition;

        Vector2 direction = (target - _enemy.GlobalPosition).Normalized();

        _projectile.GlobalPosition = _enemy.GlobalPosition;
        _projectile.Position += direction * 5f;

        _projectile.LookAt(_projectile.GlobalPosition + direction);

        _animationPlayer.Play("attack");
    }
}
