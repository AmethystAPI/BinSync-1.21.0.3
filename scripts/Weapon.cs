using System;
using Godot;
using Riptide;

public partial class Weapon : Item, Networking.NetworkNode
{
  [Export] public PackedScene ProjectileScene;

  private Networking.RpcMap _rpcMap = new Networking.RpcMap();
  public Networking.RpcMap RpcMap => _rpcMap;

  private Networking.SyncedVariable<float> _syncedRotation = new Networking.SyncedVariable<float>(nameof(_syncedRotation), 0, Networking.Authority.Client);


  public override void _Ready()
  {
    base._Ready();

    _rpcMap.Register(_syncedRotation, this);
    _rpcMap.Register(nameof(ShootRpc), ShootRpc);
  }

  public override void _Process(double delta)
  {
    _syncedRotation.Sync();

    if (Game.IsOwner(this))
    {
      _syncedRotation.Value = GlobalRotation;
    }
    else
    {
      GlobalRotation = _syncedRotation.Value;
    }

    if (!Game.IsOwner(this)) return;

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

      if (!Game.IsOwner(this)) return;

      Game.SendRpcToAllClients(this, nameof(ShootRpc), MessageSendMode.Reliable, message => { message.AddInt(4); });
    }
  }

  private void ShootRpc(Message message)
  {
    message.GetInt();

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