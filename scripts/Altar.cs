using Godot;
using Networking;
using Riptide;

public partial class Altar : Node2D, NetworkPointUser {
	[Export] public SquashAndStretch ActivateSquashAndStretch;
	[Export] public Texture2D DeactivatedTexture;
	[Export] public Texture2D ActivatedTexture;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private Area2D _interactArea;
	private Sprite2D _sprite;
	private bool _activated;

	public override void _Ready() {
		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(ActivateRpc), ActivateRpc);

		_interactArea = GetNode<Area2D>("InteractArea");
		_sprite = GetNode<Sprite2D>("Sprite");
	}

	public override void _Input(InputEvent @event) {
		if (!_activated) return;

		if (!@event.IsActionReleased("equip")) return;

		foreach (Node2D body in _interactArea.GetOverlappingBodies()) {
			if (!(body is Player)) continue;

			if (!NetworkManager.IsOwner(body)) continue;

			TrinketRealm.EnterTrinketRealm();

			Deactivate();
		}

	}

	public void Activate() {
		if (!NetworkManager.IsHost) return;

		NetworkPoint.SendRpcToClients(nameof(ActivateRpc));
	}

	public void Deactivate() {
		_activated = false;

		_sprite.Texture = DeactivatedTexture;
	}

	private void ActivateRpc(Message message) {
		_activated = true;

		ActivateSquashAndStretch.Activate();

		_sprite.Texture = ActivatedTexture;
	}
}
