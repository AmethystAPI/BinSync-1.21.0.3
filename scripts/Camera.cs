using Godot;

public partial class Camera : Camera2D {
	private static Camera s_Me;

	private float _trinketTargetInfluence;
	private float _trinketInfluence;

	public override void _Ready() {
		s_Me = this;
	}

	public override void _Process(double delta) {
		_trinketInfluence = Mathf.Lerp(_trinketInfluence, _trinketTargetInfluence, (float)delta);
	}

	public override void _PhysicsProcess(double delta) {
		if (Player.LocalPlayer == null) return;

		Vector2 mouseOffset = GetGlobalMousePosition() - Player.LocalPlayer.GlobalPosition;
		mouseOffset /= 5f;
		mouseOffset *= 1f - _trinketInfluence;

		Vector2 trinketOffset = Vector2.Up * 60f;
		trinketOffset *= _trinketInfluence;

		GlobalPosition = Player.LocalPlayer.GlobalPosition + mouseOffset + trinketOffset;
	}

	public static void EnterTrinketRealm() {
		s_Me._trinketTargetInfluence = 1f;
	}

	public static void LeaveTrinketRealm() {
		s_Me._trinketTargetInfluence = 0f;
	}
}
