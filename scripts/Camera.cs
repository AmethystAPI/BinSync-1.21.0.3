using Godot;

public partial class Camera : Camera2D {
	public static Camera Me;

	public override void _Ready() {
		Me = this;
	}

	public override void _PhysicsProcess(double delta) {
		if (Player.LocalPlayer == null) return;

		Vector2 mouseOffset = GetGlobalMousePosition() - Player.LocalPlayer.GlobalPosition;
		mouseOffset /= 5f;

		GlobalPosition = Player.LocalPlayer.GlobalPosition + mouseOffset;
	}
}
