using Godot;

public partial class Camera : Camera2D {
    public static Camera Me;

    private float _shakeTimer = 0f;
    private Vector2 _shakeDirection = Vector2.Right;
    private Vector2 _basePosition;
    private Vector2 _shakePosition;
    private float _shakeIntensity = 0f;
    private RandomNumberGenerator _random = new RandomNumberGenerator();
    private Vector2 _mouseOffset;

    public override void _Ready() {
        Me = this;
    }

    public override void _Process(double delta) {
        Vector2 scales = GetViewportRect().Size / 480;

        float scale = Mathf.Ceil(Mathf.Max(scales.X, scales.Y));

        Zoom = Vector2.One * scale;

        _shakeTimer -= (float)delta;

        if (_shakeTimer <= 0) {
            _shakeTimer = _random.RandfRange(0.02f, 0.03f);

            _shakeDirection = Vector2.Right.Rotated(_random.RandfRange(0f, Mathf.Pi * 2f));
        }

        _shakePosition = _shakeDirection * Mathf.Pow(_shakeIntensity * 3f, 0.8f);

        _shakeIntensity -= (float)delta * 8f;
        if (_shakeIntensity < 0f) _shakeIntensity = 0f;

        GlobalPosition = _basePosition + _shakePosition;
    }

    public override void _PhysicsProcess(double delta) {
        if (Player.LocalPlayer == null) return;

        Vector2 mouseScreenPosition = DisplayServer.MouseGetPosition();
        Rect2 windowRect = new Rect2(DisplayServer.WindowGetPosition(), DisplayServer.WindowGetSize());

        if (windowRect.HasPoint(mouseScreenPosition)) {
            _mouseOffset = GetGlobalMousePosition() - Player.LocalPlayer.GlobalPosition;
            _mouseOffset /= 5f;
        }

        _basePosition = Player.LocalPlayer.GlobalPosition + _mouseOffset;
    }

    public static void Shake(float intensity) {
        Me._shakeIntensity = Mathf.Max(intensity, Me._shakeIntensity);
    }
}
