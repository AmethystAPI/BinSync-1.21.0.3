using Godot;
using Networking;
using Riptide;

public partial class Altar : Node2D, NetworkPointUser {

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private Area2D _interactArea;

	public override void _Ready() {
		NetworkPoint.Setup(this);

		_interactArea = GetNode<Area2D>("InteractArea");
	}

	public override void _Input(InputEvent @event) {
		if (!@event.IsActionReleased("equip")) return;

		foreach (Node2D body in _interactArea.GetOverlappingBodies()) {
			if (!(body is Player player)) continue;

			if (!NetworkManager.IsOwner(body)) continue;

			player.EnterTrinketRealm();

			TrinketRealm.EnterTrinketRealm(this);
		}

	}
}
