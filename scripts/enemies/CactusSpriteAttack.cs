using Godot;
using Networking;
using Riptide;

public partial class CactusSpriteAttack : State {
    [Export] public PackedScene ProjectileScene;

    private Enemy _enemy;

    public override void _Ready() {
        _enemy = GetParent().GetParent<Enemy>();
    }

    public override void Enter() {
        // AnimationPlayer.Play(RunAnimation);

        Projectile _projectile = ProjectileScene.Instantiate<Projectile>();

        _projectile.Source = _enemy;

        _enemy.GetParent().AddChild(_projectile);

        Vector2 target = _enemy.GetWeightedTargets()[0].Player.GlobalPosition;

        Vector2 direction = (target - _enemy.GlobalPosition).Normalized();

        _projectile.GlobalPosition = _enemy.GlobalPosition;
        _projectile.Position += direction * 5f;

        _projectile.LookAt(_projectile.GlobalPosition + direction);

        GoToState("RangedApproach");
    }
}
