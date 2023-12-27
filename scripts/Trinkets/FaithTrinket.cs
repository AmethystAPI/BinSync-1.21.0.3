using Godot;

public partial class FaithTrinket : Trinket
{
  private ulong _lastTick;

  public override void _Process(double delta)
  {
    if (!_equipped) return;

    ulong now = Time.GetTicksMsec();

    if (now - _lastTick < 100) return;

    _lastTick = now;

    bool healedOtherPlayer = false;
    foreach (Player player in Player.Players)
    {
      if (_equippingPlayer == player) continue;

      if (_equippingPlayer.GlobalPosition.DistanceSquaredTo(player.GlobalPosition) > 1094) continue;

      healedOtherPlayer = true;

      player.ModifyHealth(0.05f);
    }

    if (!healedOtherPlayer) return;

    _equippingPlayer.ModifyHealth(0.05f);
  }
}