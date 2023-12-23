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
    Projectile projectile = ProjectileScene.Instantiate<Projectile>();

    projectile.GlobalPosition = GlobalPosition;
    projectile.Rotation = Rotation;

    Player player = GetParent().GetParent<Player>();

    projectile.SetMultiplayerAuthority(GetMultiplayerAuthority());
    projectile.Source = player;
    projectile.InheritedVelocity = player.Velocity;

    player.GetParent().AddChild(projectile);
  }
}