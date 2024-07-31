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
  [Export] public float KnockbackResistance = 0f;
  [Export] public Node2D FacingTransform;
  [Export] public Node2D VerticalTransform;
  [Export] public PackedScene DeathParticle;

  public AnimationPlayer AnimationPlayer;
  public AnimationPlayer HurtAnimationPlayer;
  public SquashAndStretch SquashAndStretch;

  public bool Activated;
  public Vector2 Knockback;
  public bool Dead;

  public bool Hurt => Knockback.Length() >= 3.5f;

  public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

  internal NetworkedVariable<Vector2> _networkedPosition = new NetworkedVariable<Vector2>(Vector2.Zero);

  protected StateMachine _stateMachine;

  private bool _justHit;
  private PackedScene _damageNumber;

  public override void _Ready() {
    NetworkPoint.Setup(this);

    NetworkPoint.Register(nameof(_networkedPosition), _networkedPosition);
    NetworkPoint.Register(nameof(DamageRpc), DamageRpc);
    NetworkPoint.Register(nameof(ActivateRpc), ActivateRpc);

    AnimationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
    HurtAnimationPlayer = GetNodeOrNull<AnimationPlayer>("HurtAnimationPlayer");
    SquashAndStretch = GetNodeOrNull<SquashAndStretch>("SquashAndStretch");

    _stateMachine = new StateMachine(GetDefaultState());
    AddStates();
    _stateMachine._Ready();

    GetParent<Room>().AddEnemy();

    _damageNumber = ResourceLoader.Load<PackedScene>("res://scenes/damage_number.tscn");
  }

  public override void _Process(double delta) {
    _stateMachine._Process(delta);

    SyncPosition((float)delta);

    _networkedPosition.Sync();
  }

  public override void _PhysicsProcess(double delta) {
    if (!Dead) Knockback = Knockback.Lerp(Vector2.Zero, (float)delta * 12f);

    _stateMachine._PhysicsProcess(delta);
  }

  public virtual void AddStates() {
    _stateMachine.Add(new Dead("dead", this));
  }

  public virtual void SyncPosition(float delta) {
    if (NetworkPoint.IsOwner) {
      _networkedPosition.Value = GlobalPosition;
    } else if (_networkedPosition.Synced) {
      if (_networkedPosition.Value.DistanceSquaredTo(GlobalPosition) > 100) GlobalPosition = _networkedPosition.Value;

      GlobalPosition = GlobalPosition.Lerp(_networkedPosition.Value, delta * 16.0f);
    }
  }

  public virtual bool CanDamage(Projectile projectile) {
    if (!Activated && Room.Current != GetParent<Room>()) return false;

    if (Hurt) return false;

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

  protected virtual string GetDefaultState() {
    return "idle";
  }

  protected virtual void DamageRpc(Message message) {
    _justHit = false;

    SetMultiplayerAuthority(message.GetInt());

    Knockback = new Vector2(message.GetFloat(), message.GetFloat());

    float damage = message.GetFloat();

    Health -= damage;

    PlayHurtEffects(damage);

    if (Health > 0) return;

    if (Dead) return;

    Dead = true;

    _stateMachine.GoToState("dead");
  }

  protected virtual void PlayHurtEffects(float damage) {
    HurtAnimationPlayer.Play("hurt");

    SquashAndStretch.Trigger(new Vector2(1.4f, 0.6f), 10f);

    DamageNumber damageNumber = _damageNumber.Instantiate<DamageNumber>();
    damageNumber.Damage = damage;
    damageNumber.Color = new Color("#ffffff");
    damageNumber.BorderColor = new Color("#fc0045");

    if (Health <= 0f) {
      damageNumber.Color = new Color("#000000");
      damageNumber.BorderColor = new Color("#ffffff");
    }

    GetParent().AddChild(damageNumber);

    damageNumber.GlobalPosition = GlobalPosition + Vector2.Up * 8f;
  }

  protected virtual void ActivateRpc(Message message) {
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