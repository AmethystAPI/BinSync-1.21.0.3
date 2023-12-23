using Godot;

public partial class Weapon : Node2D
{
  public override void _Process(double delta)
  {
    if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;

    LookAt(GetGlobalMousePosition());
  }
}