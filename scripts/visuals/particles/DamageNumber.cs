using Godot;

public partial class DamageNumber : Node2D {
	[Export] public Label Label;
	[Export] public SquashAndStretch SquashAndStretch;

	public float Damage = 0;
	public Color Color = new Color(1f, 1f, 1f);
	public Color BorderColor = new Color(0f, 0f, 0f);

	private Vector2 _velocity;
	private float _gravity = 0f;

	public override void _Ready() {
		SquashAndStretch.Trigger(new Vector2(1.4f, 0.6f), 6f);

		RandomNumberGenerator random = new RandomNumberGenerator();

		_velocity = Vector2.Up.Rotated(random.RandfRange(-Mathf.Pi / 4f, Mathf.Pi / 4f)) * random.RandfRange(100, 150f);

		Label.Modulate = Color;
		(Label.Material as ShaderMaterial).SetShaderParameter("border_color", BorderColor);
		Label.Text = (Mathf.Round(Damage * 5f * 100f) / 100f).ToString();
	}

	public override void _Process(double delta) {
		_velocity = MathHelper.FixedLerp(_velocity, Vector2.Zero, 4f, (float)delta);
		_gravity += (float)delta * 100f;

		GlobalPosition += (_velocity + Vector2.Down * _gravity) * (float)delta;

		Color color = Label.Modulate;
		color.A = MathHelper.FixedLerp(color.A, 0f, 6f, (float)delta);
		Label.Modulate = color;

		if (color.A <= 0.01f) QueueFree();
	}
}
