using Godot;

public partial class Weapon : Node2D
{
  [Export] public PackedScene ProjectileScene;

  public override void _Process(double delta)
  {
    if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;

    LookAt(GetGlobalMousePosition());
  }

  public override void _Input(InputEvent @event)
  {
    if (!@event.IsActionPressed("shoot")) return;

    if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;

    Rpc(nameof(ShootRpc));
  }

  [Rpc(CallLocal = true)]
  private void ShootRpc()
  {
    Node2D projectile = ProjectileScene.Instantiate<Node2D>();

    projectile.GlobalPosition = GlobalPosition;
    projectile.Rotation = Rotation;

    projectile.SetMultiplayerAuthority(GetMultiplayerAuthority());

    GetParent().GetParent().AddChild(projectile);
  }
}