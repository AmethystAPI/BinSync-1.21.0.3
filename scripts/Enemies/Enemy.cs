using Godot;
using Networking;
using Riptide;

public partial class Enemy : CharacterBody2D, Damageable, NetworkPointUser
{
  [Export] public float Health = 3f;

  public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

  internal NetworkedVariable<Vector2> _networkedPosition = new NetworkedVariable<Vector2>(Vector2.Zero);

  internal Vector2 _knockback;

  public override void _Ready()
  {
    NetworkPoint.Setup(this);

    NetworkPoint.Register(nameof(_networkedPosition), _networkedPosition);
    NetworkPoint.Register(nameof(DamageRpc), DamageRpc);

    GetParent<Room>().AddEnemy();
  }

  public override void _Process(double delta)
  {
    _networkedPosition.Sync();

    SyncPosition((float)delta);
  }

  public virtual void SyncPosition(float delta)
  {
    if (NetworkPoint.IsOwner)
    {
      _networkedPosition.Value = GlobalPosition;
    }
    else
    {
      if (_networkedPosition.Value.DistanceSquaredTo(GlobalPosition) > 64) GlobalPosition = _networkedPosition.Value;

      GlobalPosition = GlobalPosition.Lerp(_networkedPosition.Value, delta * 20.0f);
    }
  }

  public virtual bool CanDamage(Projectile projectile)
  {
    return projectile.Source is Player;
  }

  public void Damage(Projectile projectile)
  {
    if (!NetworkPoint.IsOwner) return;

    NetworkPoint.BounceRpcToClients(nameof(DamageRpc), message =>
    {
      message.AddInt(projectile.GetMultiplayerAuthority());

      Vector2 knockback = projectile.GlobalTransform.BasisXform(Vector2.Right) * 200f * projectile.Knockback;

      message.AddFloat(knockback.X);
      message.AddFloat(knockback.Y);

      message.AddFloat(projectile.Damage);
    });
  }

  private void DamageRpc(Message message)
  {
    SetMultiplayerAuthority(message.GetInt());

    _knockback = new Vector2(message.GetFloat(), message.GetFloat());

    Health -= message.GetFloat();

    if (Health <= 0)
    {
      GetParent<Room>().RemoveEnemy();

      QueueFree();
    }
  }
}