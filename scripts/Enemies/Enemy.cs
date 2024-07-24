using System.Linq;
using Godot;
using Networking;
using Riptide;

public partial class Enemy : CharacterBody2D, Damageable, NetworkPointUser {
  public class WeightedTarget {
    public Player Player;
    public float Weight;
  }

  [Export] public float Health = 3f;
  [Export] public Node2D FacingTransform;

  public AnimationPlayer AnimationPlayer;

  public bool Activated;

  public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

  internal NetworkedVariable<Vector2> _networkedPosition = new NetworkedVariable<Vector2>(Vector2.Zero);

  protected StateMachine _stateMachine = new StateMachine("idle");

  private bool _justHit;
  private float _invincibilityTimer;

  public override void _Ready() {
    NetworkPoint.Setup(this);

    NetworkPoint.Register(nameof(_networkedPosition), _networkedPosition);
    NetworkPoint.Register(nameof(DamageRpc), DamageRpc);
    NetworkPoint.Register(nameof(ActivateRpc), ActivateRpc);

    AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

    AddStates();
    _stateMachine._Ready();

    GetParent<Room>().AddEnemy();
  }

  public override void _Process(double delta) {
    _stateMachine._Process(delta);
    _networkedPosition.Sync();

    SyncPosition((float)delta);

    if (_stateMachine.CurrentState != "Hurt") _invincibilityTimer -= (float)delta;
  }

  public override void _PhysicsProcess(double delta) {
    _stateMachine._PhysicsProcess(delta);
  }

  public virtual void AddStates() {

  }

  public virtual void SyncPosition(float delta) {
    if (NetworkPoint.IsOwner) {
      _networkedPosition.Value = GlobalPosition;
    } else {
      if (_networkedPosition.Value.DistanceSquaredTo(GlobalPosition) > 100) GlobalPosition = _networkedPosition.Value;

      GlobalPosition = GlobalPosition.Lerp(_networkedPosition.Value, delta * 16.0f);
    }
  }

  public virtual bool CanDamage(Projectile projectile) {
    if (!Activated && Room.Current != GetParent<Room>()) return false;

    if (_stateMachine.CurrentState == "Hurt") return false;

    if (_invincibilityTimer >= 0f) return false;

    if (!(projectile.Source is Player)) return false;


    return true;
  }

  public void Damage(Projectile projectile) {
    if (!NetworkManager.IsOwner(projectile)) return;

    if (_justHit) return;

    _justHit = true;

    if (projectile.Source is Player player) {
      foreach (Trinket trinket in player.EquippedTrinkets) {
        trinket.HitEnemy(this, projectile);
      }
    }

    NetworkPoint.BounceRpcToClientsFast(nameof(DamageRpc), message => {
      message.AddInt(projectile.GetMultiplayerAuthority());

      Vector2 knockback = projectile.GlobalTransform.BasisXform(Vector2.Right) * 200f * projectile.Knockback;

      message.AddFloat(knockback.X);
      message.AddFloat(knockback.Y);

      message.AddFloat(projectile.GetDamage());
    });
  }

  public void Activate() {
    if (!NetworkManager.IsHost) return;

    if (!IsInstanceValid(this)) return;

    NetworkPoint.SendRpcToClients(nameof(ActivateRpc));
  }

  private void DamageRpc(Message message) {
    _justHit = false;

    _invincibilityTimer = 0.1f;

    SetMultiplayerAuthority(message.GetInt());

    _stateMachine.GetState<Hurt>("Hurt").Knockback = new Vector2(message.GetFloat(), message.GetFloat());

    Health -= message.GetFloat();

    _stateMachine.GoToState("Hurt");
  }

  private void ActivateRpc(Message message) {
    Activated = true;
  }

  public WeightedTarget[] GetWeightedTargets() {
    return Player.AlivePlayers.Select(player => new WeightedTarget {
      Player = player,
      Weight = GlobalPosition.DistanceTo(player.GlobalPosition) - player.EquippedTrinkets.Where(trinket => trinket is PerfumeTrinket).Count() * 48f
    }).OrderBy(target => target.Weight).ToArray();
  }

  public void Face(bool right) {
    FacingTransform.Scale = new Vector2(right ? 1f : -1f, 1f);
  }

  public void Face(Vector2 position) {
    FacingTransform.Scale = new Vector2(position.X > GlobalPosition.X ? 1f : -1f, 1f);
  }
}