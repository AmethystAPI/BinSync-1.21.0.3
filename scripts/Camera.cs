using Godot;

public partial class Camera : Camera2D
{
	public override void _PhysicsProcess(double delta)
	{
		if (Player.LocalPlayer == null) return;

		GlobalPosition = Player.LocalPlayer.GlobalPosition;
	}
}
