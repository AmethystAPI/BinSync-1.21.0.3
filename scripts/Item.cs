using Godot;
using Riptide;

public partial class Item : Node2D
{
  internal bool _equipped;
  internal Player _equippingPlayer;

  private Area2D _equipArea;

  public override void _Ready()
  {
    _equipArea = GetNode<Area2D>("EquipArea");
  }

  public override void _Input(InputEvent @event)
  {
    if (@event.IsActionPressed("equip"))
    {
      if (_equipped) return;

      foreach (Node2D body in _equipArea.GetOverlappingBodies())
      {
        if (!(body is Player)) continue;

        if (!Game.IsOwner(body)) continue;

        if ((body as Player).Health <= 0) continue;

        (body as Player).Equip(this);

        break;
      }
    }
  }

  public virtual void Equip(Player player)
  {
    _equipped = true;
    _equippingPlayer = player;
  }
}