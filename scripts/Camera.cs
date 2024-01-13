using Godot;

public partial class Camera : Camera2D {
	public override void _PhysicsProcess(double delta) {
		if (Player.LocalPlayer == null) return;

		Vector2 mouseOffset = GetGlobalMousePosition() - Player.LocalPlayer.GlobalPosition;

		GlobalPosition = Player.LocalPlayer.GlobalPosition + mouseOffset / 6f;
	}
}
