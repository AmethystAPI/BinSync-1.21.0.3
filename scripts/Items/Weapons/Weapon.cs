using Godot;
using Networking;

public partial class Weapon : Item, Interactable {
  [Export] public float PickupRange = 16f;

  internal bool _shootPressed = false;

  private NetworkedVariable<float> _syncedRotation = new NetworkedVariable<float>(0);

  private Node2D _heldNode;
  private Node2D _pickupNode;

  public override void _Ready() {
    base._Ready();

    NetworkPoint.Register(nameof(_syncedRotation), _syncedRotation);

    _heldNode = GetNode<Node2D>("Held");
    _pickupNode = GetNode<Node2D>("Pickup");

    _pickupNode.Visible = true;
    _heldNode.Visible = false;
  }

  public override void _Process(double delta) {
    _syncedRotation.Sync();

    if (NetworkPoint.IsOwner) {
      _syncedRotation.Value = GlobalRotation;
    } else {
      GlobalRotation = _syncedRotation.Value;
    }

    if (!NetworkPoint.IsOwner) return;

    if (_equipped) {
      LookAt(GetGlobalMousePosition());

    } else {
      Rotation = 0f;
    }
  }

  public override void _Input(InputEvent @event) {
    base._Input(@event);

    if (@event.IsActionPressed("shoot")) {
      if (!NetworkPoint.IsOwner) return;

      if (!_equipped) return;

      if (_equippingPlayer.Health <= 0) return;

      _shootPressed = true;

      ShootPressed();
    }

    if (@event.IsActionReleased("shoot") && _shootPressed) {
      _shootPressed = false;

      ShootReleased();
    }
  }

  public override void OnEquipToPlayer(Player player) {
    base.OnEquipToPlayer(player);

    _pickupNode.Visible = false;
    _heldNode.Visible = true;
  }

  public virtual void ShootPressed() {

  }

  public virtual void ShootReleased() {

  }

  public virtual void CancelShoot() {

  }

  public bool CanInteract(Node2D interactor) {
    if (_equipped) return false;

    return interactor.GlobalPosition.DistanceTo(GlobalPosition) <= PickupRange;
  }

  public void Interact(Node2D interactor) {
    Player.LocalPlayer.Equip(this);
  }

  public void ActivateInteract() {

  }
  public void InactivateInteract() {

  }
}