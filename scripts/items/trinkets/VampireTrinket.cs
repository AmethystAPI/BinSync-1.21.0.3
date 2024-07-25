using Godot;

public partial class VampireTrinket : Trinket {
    private RandomNumberGenerator _random = new();

    public override void HitEnemy(Enemy enemy, Projectile projectile) {
        if (_random.RandiRange(1, 7) != 7) return;

        _equippingPlayer.Heal(projectile.Damage * 0.2f);
    }
}