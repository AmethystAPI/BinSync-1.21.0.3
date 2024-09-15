using System;
using Godot;
using Networking;
using Riptide;

public partial class SimpleWeapon : Weapon {
  [Signal] public delegate void ShootEventHandler();

  [Export] public PackedScene ProjectileScene;
  [Export] public float Delay = 0.25f;
  [Export] public SquashAndStretch SquashAndStretch;
  [Export] public Vector2 SquashAndStretchScale = new Vector2(2f, 0.4f);
  [Export] public float SquashAndStretchSpeed = 12f;
  [Export] public float ProjectileOffset = 6f;

  private float _shootTimer;

  public override void _Ready() {
    base._Ready();

    NetworkPoint.Register(nameof(ShootRpc), ShootRpc);
  }

  public override void _Process(double delta) {
    base._Process(delta);

    if (!NetworkPoint.IsOwner) return;

    _shootTimer -= (float)delta;

    if (!_equipped) return;

    if (!_shootPressed) return;

    if (_shootTimer > 0) return;

    _shootTimer = Delay;

    NetworkPoint.BounceRpcToClientsFast(nameof(ShootRpc));

    Camera.Shake(0.2f);
  }

  public override void CancelShoot() {
    _shootPressed = false;
  }

  private void ShootRpc(Message message) {
    EmitSignal(SignalName.Shoot);

    int dualProjectiles = _equippingPlayer.GetTrinketCount("duality");

    for (int index = 0; index < 1 + dualProjectiles; index++) {
      Projectile projectile = ProjectileScene.Instantiate<Projectile>();

      float rotationOffset = Mathf.Pi * 2f / (1 + dualProjectiles) * index;

      projectile.GlobalPosition = GlobalPosition + Vector2.Right.Rotated(Rotation + rotationOffset) * ProjectileOffset;
      projectile.Rotation = Rotation + rotationOffset;

      projectile.SetMultiplayerAuthority(GetMultiplayerAuthority());
      projectile.Source = _equippingPlayer;
      projectile.InheritedVelocity = _equippingPlayer.Velocity;

      _equippingPlayer.GetParent().AddChild(projectile);

      Trinket.ModifyProjectile(this, projectile);

      // DEBUG
      // projectile.Damage = 999f;
    }

    if (SquashAndStretch != null) SquashAndStretch.Trigger(SquashAndStretchScale, SquashAndStretchSpeed);

    Audio.Play("weapon_shoot");
  }
}