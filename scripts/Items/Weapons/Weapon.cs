using System;
using Godot;
using Networking;
using Riptide;

public partial class Weapon : Item
{
  private NetworkedVariable<float> _syncedRotation = new NetworkedVariable<float>(0);
  internal bool _shootPressed = false;

  public override void _Ready()
  {
    base._Ready();

    NetworkPoint.Register(nameof(_syncedRotation), _syncedRotation);
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
      if (!NetworkPoint.IsOwner) return;

      if (!_equipped) return;

      if (_equippingPlayer.Health <= 0) return;

      _shootPressed = true;

      ShootPressed();
    }

    if (@event.IsActionReleased("shoot") && _shootPressed)
    {
      _shootPressed = false;

      ShootReleased();
    }
  }

  public virtual void ShootPressed()
  {

  }

  public virtual void ShootReleased()
  {

  }
}