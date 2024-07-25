using System.Linq;
using Godot;

public partial class FaithTrinket : Trinket {
  private float timer = 2f;

  public override void _Process(double delta) {
    base._Process(delta);

    if (!_equipped) return;

    if (_equippingPlayer.Health <= 0) return;

    timer -= (float)delta;

    if (timer <= 0f) {
      timer = 2f;

      if (GetTree().GetNodesInGroup("Enemies").Where(node => node is Node2D node2D && node2D.GlobalPosition.DistanceTo(_equippingPlayer.GlobalPosition) < 48f).Count() > 0) {
        _equippingPlayer.Heal(0.2f);
      }
    }
  }
}