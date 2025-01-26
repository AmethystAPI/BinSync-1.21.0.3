using Godot;

public partial class FreeAfterDelay : Node2D {
	[Export] public float Timer;

	public override void _Process(double delta) {
		Timer -= (float)delta;

		if (Timer <= 0f) GetParent().QueueFree();
	}
}
