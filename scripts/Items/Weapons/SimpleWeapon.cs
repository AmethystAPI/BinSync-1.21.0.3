using System;
using Godot;
using Networking;
using Riptide;

public partial class SimpleWeapon : Weapon {
  [Signal] public delegate void ShootEventHandler();

  [Export] public PackedScene ProjectileScene;
  [Export] public float Delay = 0.25f;

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
  }

  public override void CancelShoot() {
    _shootPressed = false;
  }

  private void ShootRpc(Message message) {
    EmitSignal(SignalName.Shoot);

    Projectile projectile = ProjectileScene.Instantiate<Projectile>();

    projectile.GlobalPosition = GlobalPosition;
    projectile.Rotation = Rotation;

    projectile.SetMultiplayerAuthority(GetMultiplayerAuthority());
    projectile.Source = _equippingPlayer;
    projectile.InheritedVelocity = _equippingPlayer.Velocity;

    _equippingPlayer.GetParent().AddChild(projectile);

    foreach (Trinket trinket in _equippingPlayer.EquippedTrinkets) {
      trinket.ModifyProjectile(this, projectile);
    }

    // DEBUG
    // projectile.Damage = 999f;

    Audio.Play("weapon_shoot");
  }
}