public interface Damageable
{
  public void Damage(Projectile projectile);

  public bool CanDamage(Projectile projectile);
}