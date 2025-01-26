using Godot;
using Networking;

public partial class Altar : Node2D, NetworkPointUser, Interactable {

	[Export] public float InteractRange = 32f;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private bool _activated = false;

	public override void _Ready() {
		NetworkPoint.Setup(this);
	}

	public bool CanInteract(Node2D interactor) {
		if (_activated) return false;

		return interactor.GlobalPosition.DistanceTo(GlobalPosition) <= InteractRange;
	}

	public void Interact(Node2D interactor) {
		if (!(interactor is Player player)) return;

		_activated = true;

		player.EnterTrinketRealm();
	}
}
