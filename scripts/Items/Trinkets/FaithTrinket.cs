using Godot;

public partial class FaithTrinket : Trinket {
  private ulong _lastTick;

  public override void _Process(double delta) {
    base._Process(delta);

    if (!_equipped) return;

    if (_equippingPlayer.Health <= 0) return;

    ulong now = Time.GetTicksMsec();

    if (now - _lastTick < 100) return;

    _lastTick = now;

    foreach (Player player in Player.AlivePlayers) {
      if (_equippingPlayer.GlobalPosition.DistanceSquaredTo(player.GlobalPosition) > 1094) continue;

      player.Heal(0.03f);
    }
  }
}