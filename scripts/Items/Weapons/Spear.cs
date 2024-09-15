using System;
using System.Collections.Generic;
using Godot;
using Networking;
using Riptide;

public partial class Spear : Weapon {
  [Export] public PackedScene ProjectileScene;
  [Export] public float Delay = 0.25f;
  [Export] public float ProjectileSeperation = 0.05f;

  private float _shootTimer;

  private List<float> _shootQueue = new List<float>();

  public override void _Ready() {
    base._Ready();

    NetworkPoint.Register(nameof(ShootRpc), ShootRpc);
  }

  public override void _Process(double delta) {
    base._Process(delta);

    if (!NetworkPoint.IsOwner) return;

    _shootTimer -= (float)delta;

    if (!_equipped) return;

    for (int index = 0; index < _shootQueue.Count; index++) {
      _shootQueue[index] -= (float)delta;

      if (_shootQueue[index] > 0) continue;

      Shoot();

      _shootQueue.RemoveAt(index);

      index--;
    }

    if (!_shootPressed) return;

    if (_shootTimer > 0) return;

    _shootTimer = Delay;

    Shoot();

    _shootQueue.Add(ProjectileSeperation);

    // _shootQueue.Add(ProjectileSeperation * 2f);
  }

  public override void CancelShoot() {
    _shootPressed = false;
  }

  private void Shoot() {
    NetworkPoint.BounceRpcToClientsFast(nameof(ShootRpc));
  }

  private void ShootRpc(Message message) {
    int dualProjectiles = _equippingPlayer.GetTrinketCount("duality");

    for (int index = 0; index < 1 + dualProjectiles; index++) {
      Projectile projectile = ProjectileScene.Instantiate<Projectile>();

      float rotationOffset = Mathf.Pi * 2f / (1 + dualProjectiles) * index;

      projectile.GlobalPosition = GlobalPosition;
      projectile.Rotation = Rotation + rotationOffset;

      projectile.SetMultiplayerAuthority(GetMultiplayerAuthority());
      projectile.Source = _equippingPlayer;
      projectile.InheritedVelocity = _equippingPlayer.Velocity;

      _equippingPlayer.GetParent().AddChild(projectile);

      Trinket.ModifyProjectile(this, projectile);
    }
  }
}