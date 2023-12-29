public partial class StrengthTrinket : Trinket
{
  public override void ModifyProjectile(Weapon weapon, Projectile projectile)
  {
    projectile.Damage *= 2f;
  }
}