using System;
using Godot;
using Networking;

public partial class Item : Node2D, NetworkPointUser {
  public Action Equipped;

  public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

  internal bool _equipped;
  internal Player _equippingPlayer;

  public override void _Ready() {
    NetworkPoint.Setup(this);
  }

  public virtual void OnEquipToPlayer(Player player) {
    Position = Vector2.Zero;

    _equipped = true;
    _equippingPlayer = player;

    Equipped?.Invoke();
  }
}