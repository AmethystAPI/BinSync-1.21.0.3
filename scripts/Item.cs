using Godot;
using Networking;

public partial class Item : Node2D, NetworkPointUser
{
  public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

  internal bool _equipped;
  internal Player _equippingPlayer;

  private Area2D _equipArea;

  public override void _Ready()
  {
    NetworkPoint.Setup(this);

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

        Player player = (Player)body;

        if (!player.NetworkPoint.IsOwner) continue;

        if (player.Health <= 0) continue;

        player.Equip(this);

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