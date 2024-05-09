using Godot;
using Networking;
using Riptide;

public partial class Altar : Node2D, NetworkPointUser {

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private Area2D _interactArea;

	private bool _activated = false;

	public override void _Ready() {
		NetworkPoint.Setup(this);

		_interactArea = GetNode<Area2D>("InteractArea");
	}

	public override void _Input(InputEvent @event) {
		if (_activated) return;

		if (!@event.IsActionReleased("equip")) return;

		foreach (Node2D body in _interactArea.GetOverlappingBodies()) {
			if (!(body is Player player)) continue;

			if (!NetworkManager.IsOwner(body)) continue;

			_activated = true;

			player.EnterTrinketRealm();

			TrinketRealm.EnterTrinketRealm(this);
		}

	}
}
