using Godot;

public partial class FaithTrinket : Trinket
{
  public override void _Process(double delta)
  {
    if (!_equipped) return;

    bool healedOtherPlayer = false;
    foreach (Player player in Player.Players)
    {
      if (_equippingPlayer == player) continue;

      if (_equippingPlayer.GlobalPosition.DistanceSquaredTo(player.GlobalPosition) > 1094) continue;

      healedOtherPlayer = true;

      player.ModifyHealth((float)delta / 2f);
    }

    if (!healedOtherPlayer) return;

    _equippingPlayer.ModifyHealth((float)delta / 2f);
  }
}