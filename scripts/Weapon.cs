using Godot;
using Riptide;

public partial class Weapon : Node2D, Networking.NetworkNode
{
  [Export] public PackedScene ProjectileScene;

  private Networking.RpcMap _rpcMap = new Networking.RpcMap();
  public Networking.RpcMap RpcMap => _rpcMap;

  private Networking.SyncedVariable<float> _syncedRotation = new Networking.SyncedVariable<float>(nameof(_syncedRotation), 0, Networking.Authority.Client);

  private bool _equipped;
  private Area2D _equipArea;

  public override void _Ready()
  {
    _rpcMap.Register(_syncedRotation, this);
    _rpcMap.Register(nameof(ShootRpc), ShootRpc);

    _equipArea = GetNode<Area2D>("EquipArea");
  }

  public override void _Process(double delta)
  {
    _syncedRotation.Sync();

    if (Game.IsOwner(this))
    {
      _syncedRotation.Value = GlobalRotation;
    }
    else
    {
      GlobalRotation = _syncedRotation.Value;
    }

    if (!Game.IsOwner(this)) return;

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

        if (!Game.IsOwner(body)) continue;

        (body as Player).EquipWeapon(this);

        break;
      }
    }

    if (@event.IsActionPressed("shoot"))
    {
      if (!_equipped) return;

      if (!Game.IsOwner(this)) return;

      Game.SendRpcToOtherClients(this, nameof(ShootRpc), MessageSendMode.Reliable, message => { });
    }
  }

  public void Equip()
  {
    _equipped = true;
  }

  private void ShootRpc(Message message)
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