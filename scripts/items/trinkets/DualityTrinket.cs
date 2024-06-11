using Godot;

public partial class DualityTrinket : Trinket {
  public override void ModifyProjectile(Weapon weapon, Projectile projectile) {
    Projectile newProjectile = projectile.Duplicate(8) as Projectile;

    projectile.GetParent().AddChild(newProjectile);

    newProjectile.SetMultiplayerAuthority(projectile.GetMultiplayerAuthority());
    newProjectile.Source = projectile.Source;
    newProjectile.InheritedVelocity = _equippingPlayer.Velocity;

    if (newProjectile is BoomerangProjectile boomerangProjectile) {
      boomerangProjectile.Velocity = boomerangProjectile.Velocity.Rotated(Mathf.Pi);

      return;
    }

    newProjectile.Rotate(Mathf.Pi);
  }
}