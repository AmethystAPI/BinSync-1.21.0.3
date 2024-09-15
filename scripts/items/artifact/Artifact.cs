using Godot;
using Networking;

public partial class Artifact : Item {
	[Export] public float Cooldown = 10f;

	private float _cooldownTimer;

	public override void _Ready() {
		base._Ready();
	}

	public override void _Process(double delta) {
		if (!_equipped) return;

		_cooldownTimer -= (float)delta;

		float time = Time.GetTicksMsec() / 1000f;

		Position = new Vector2(24f * Mathf.Cos(time * 0.5f), 12f * Mathf.Sin(time * 0.5f));
	}

	public override void _Input(InputEvent @event) {
		base._Input(@event);

		if (@event.IsActionPressed("use_artifact")) {
			if (!NetworkPoint.IsOwner) return;

			if (!_equipped) return;

			if (_equippingPlayer.Health <= 0) return;

			if (_cooldownTimer > 0) return;

			_cooldownTimer = Cooldown;

			Activate();
		}
	}

	public virtual void Activate() {
		GD.Print("Activate!");
	}
}
