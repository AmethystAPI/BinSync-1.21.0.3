using Godot;

public partial class GameUI : Control
{
	private static GameUI s_Me;

	[Export] public ProgressBar HealthBar;

	public override void _Ready()
	{
		s_Me = this;
	}

	public static void UpdateHealth(float health)
	{
		s_Me.HealthBar.Value = health;
	}
}
