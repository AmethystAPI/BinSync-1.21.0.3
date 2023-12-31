using Godot;
using Networking;
using Riptide;

public partial class Enemy : CharacterBody2D, Damageable, NetworkPointUser
{
  [Export] public float Health = 3f;

  public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

  internal NetworkedVariable<Vector2> _networkedPosition = new NetworkedVariable<Vector2>(Vector2.Zero);

  internal Vector2 _knockback;
  internal StateMachine _stateMachine;

  private bool _justHit;

  public override void _Ready()
  {
    NetworkPoint.Setup(this);

    NetworkPoint.Register(nameof(_networkedPosition), _networkedPosition);
    NetworkPoint.Register(nameof(DamageRpc), DamageRpc);

    _stateMachine = GetNode<StateMachine>("StateMachine");

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
      if (_networkedPosition.Value.DistanceSquaredTo(GlobalPosition) > 100) GlobalPosition = _networkedPosition.Value;

      GlobalPosition = GlobalPosition.Lerp(_networkedPosition.Value, delta * 16.0f);
    }
  }

  public virtual bool CanDamage(Projectile projectile)
  {
    if (_stateMachine.CurrentState == "Hurt") return false;

    if (!(projectile.Source is Player)) return false;

    return true;
  }

  public void Damage(Projectile projectile)
  {
    if (!NetworkManager.IsOwner(projectile)) return;

    if (_justHit) return;

    _justHit = true;

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
    _justHit = false;

    SetMultiplayerAuthority(message.GetInt());

    _stateMachine.GetState<Hurt>("Hurt").Knockback = new Vector2(message.GetFloat(), message.GetFloat());

    Health -= message.GetFloat();

    _stateMachine.GoToState("Hurt");
  }
}