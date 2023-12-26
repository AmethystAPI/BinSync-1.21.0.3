using Godot;

public partial class Weapon : Node2D
{
  [Export] public PackedScene ProjectileScene;

  private bool _equipped;
  private Area2D _equipArea;

  public override void _Ready()
  {
    _equipArea = GetNode<Area2D>("EquipArea");
  }

  public override void _Process(double delta)
  {
    if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;

    LookAt(GetGlobalMousePosition());
  }

  public override void _Input(InputEvent @event)
  {
    if (@event.IsActionPressed("equip"))
    {
      if (_equipped) return;

      foreach (Node2D body in _equipArea.GetOverlappingBodies())
      {
        if (!(body is Player)) continue;

        if (body.GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) continue;

        GD.Print("Equipping " + Name);

        (body as Player).EquipWeapon(this);

        break;
      }
    }

    if (@event.IsActionPressed("shoot"))
    {
      if (!_equipped) return;

      if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;

      Rpc(nameof(ShootRpc));
    }
  }

  public void Equip()
  {
    _equipped = true;
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