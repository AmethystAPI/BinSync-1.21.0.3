public partial class StrengthTrinket : Trinket {
  public override void ModifyProjectile(Weapon weapon, Projectile projectile) {
    projectile.Damage *= 2f;

    if (projectile is WhipProjectile whipProjectile) whipProjectile.DamageIncrease *= 1.5f;
  }
}