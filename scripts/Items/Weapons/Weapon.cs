using System;
using Godot;
using Networking;
using Riptide;

public partial class Weapon : Item
{
  [Export] public PackedScene ProjectileScene;

  private NetworkedVariable<float> _syncedRotation = new NetworkedVariable<float>(0);

  public override void _Ready()
  {
    base._Ready();

    NetworkPoint.Register(nameof(_syncedRotation), _syncedRotation);
    NetworkPoint.Register(nameof(ShootRpc), ShootRpc);
  }

  public override void _Process(double delta)
  {
    _syncedRotation.Sync();

    if (NetworkPoint.IsOwner)
    {
      _syncedRotation.Value = GlobalRotation;
    }
    else
    {
      GlobalRotation = _syncedRotation.Value;
    }

    if (!NetworkPoint.IsOwner) return;

    if (!_equipped) return;

    LookAt(GetGlobalMousePosition());
  }

  public override void _Input(InputEvent @event)
  {
    base._Input(@event);

    if (@event.IsActionPressed("shoot"))
    {
      if (!_equipped) return;

      if (_equippingPlayer.Health <= 0) return;

      if (!NetworkPoint.IsOwner) return;

      NetworkPoint.BounceRpcToClients(nameof(ShootRpc));
    }
  }

  private void ShootRpc(Message message)
  {
    Projectile projectile = ProjectileScene.Instantiate<Projectile>();

    projectile.GlobalPosition = GlobalPosition;
    projectile.Rotation = Rotation;

    projectile.SetMultiplayerAuthority(GetMultiplayerAuthority());
    projectile.Source = _equippingPlayer;
    projectile.InheritedVelocity = _equippingPlayer.Velocity;

    _equippingPlayer.GetParent().AddChild(projectile);

    foreach (Trinket trinket in _equippingPlayer.EquippedTrinkets)
    {
      trinket.ModifyProjectile(this, projectile);
    }
  }
}