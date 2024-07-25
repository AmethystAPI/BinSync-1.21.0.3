using Godot;

public partial class Camera : Camera2D {
	public static Camera Me;

	private float _shakeTimer = 0f;
	private Vector2 _shakeDirection = Vector2.Right;
	private Vector2 _basePosition;
	private Vector2 _shakePosition;
	private float _shakeIntensity = 0f;
	private RandomNumberGenerator _random = new RandomNumberGenerator();

	public override void _Ready() {
		Me = this;
	}

	public override void _Process(double delta) {
		_shakeTimer -= (float)delta;

		if (_shakeTimer <= 0) {
			_shakeTimer = _random.RandfRange(0.015f, 0.03f);

			_shakeDirection = Vector2.Right.Rotated(_random.RandfRange(0f, Mathf.Pi * 2f));
		}

		_shakePosition = _shakeDirection * Mathf.Pow(_shakeIntensity * 2f, 1.5f);

		_shakeIntensity = MathHelper.FixedLerp(_shakeIntensity, 0f, 14f, (float)delta);

		GlobalPosition = _basePosition + _shakePosition;
	}

	public override void _PhysicsProcess(double delta) {
		if (Player.LocalPlayer == null) return;

		Vector2 mouseOffset = GetGlobalMousePosition() - Player.LocalPlayer.GlobalPosition;
		mouseOffset /= 5f;

		_basePosition = Player.LocalPlayer.GlobalPosition + mouseOffset;
	}

	public static void Shake(float intensity) {
		Me._shakeIntensity = Mathf.Max(intensity, Me._shakeIntensity);
	}
}
