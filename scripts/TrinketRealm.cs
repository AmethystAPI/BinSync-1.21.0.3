using Godot;
using Networking;
using Riptide;
using System.Collections.Generic;

public partial class TrinketRealm : Node2D, NetworkPointUser {
	public static TrinketRealm Me;

	[Export] public PackedScene[] TrinketScenes = new PackedScene[] { };
	[Export] public ColorRect TrinketBackground;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private float _trinketBackgroundTargetAlpha = 0f;

	public override void _Ready() {
		Me = this;

		NetworkPoint.Setup(this);
	}

	public override void _Process(double delta) {
		Color color = Me.TrinketBackground.Modulate;
		color.A = Mathf.Lerp(color.A, _trinketBackgroundTargetAlpha, 1f * (float)delta);

		Me.TrinketBackground.Modulate = color;
	}

	public static void EnterTrinketRealm() {
		Me._trinketBackgroundTargetAlpha = 1f;
	}

	public static void LeaveTinketRealm() {
		Me._trinketBackgroundTargetAlpha = 0f;
	}
}
