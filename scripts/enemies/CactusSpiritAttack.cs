using System.Collections.Generic;
using Godot;
using Networking;
using Riptide;

public partial class CactusSpiritAttack : State {
    [Export] public PackedScene ProjectileScene;
    [Export] public Sprite2D Sprite;

    private Enemy _enemy;
    private float _timer;
    private List<float> _shootQueue = new List<float>();
    private AnimationPlayer _animationPlayer;
    private Vector2 _direction;

    public override void _Ready() {
        _enemy = GetParent().GetParent<Enemy>();
        _animationPlayer = GetParent().GetParent().GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void Enter() {
        _animationPlayer.Play("telegraph_attack");

        _timer = 1f;

        Vector2 target = _enemy.GetWeightedTargets()[0].Player.GlobalPosition;

        _direction = (target - _enemy.GlobalPosition).Normalized();

        Sprite.Scale = new Vector2(target.X > _enemy.GlobalPosition.X ? 1f : -1f, 1f);

        _shootQueue.Clear();
        _shootQueue.Add(0.3f);
        _shootQueue.Add(0.3f + 0.12f);
        _shootQueue.Add(0.3f + 0.12f * 2f);
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

        _projectile.GlobalPosition = _enemy.GlobalPosition;
        _projectile.Position += _direction * 5f;

        _projectile.LookAt(_projectile.GlobalPosition + _direction);

        _animationPlayer.Play("attack");
    }
}
