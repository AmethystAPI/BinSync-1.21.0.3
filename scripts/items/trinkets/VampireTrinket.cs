using Godot;

public partial class VampireTrinket : Trinket {
    public override void HitEnemy(Enemy enemy, Projectile projectile) {
        _equippingPlayer.Heal(projectile.Damage * 0.1f);
    }
}