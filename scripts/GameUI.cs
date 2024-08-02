using Godot;

public partial class GameUI : Control {
	private static GameUI s_Me;

	[Export] public PackedScene HeartScene;
	[Export] public HBoxContainer HeartContainer;
	[Export] public Texture2D FullHeart;
	[Export] public Texture2D HalfHeart;
	[Export] public Texture2D EmptyHeart;
	[Export] public Control BossHealthBar;
	[Export] public Control BossHealthBarFill;

	private float _bossHealthBarMax = 3f;
	private float _bossHealthBarValue = 3f;
	private bool _showBossHealthBar = false;

	public override void _Ready() {
		s_Me = this;

		for (int i = 0; i < 3; i++) {
			Node heart = HeartScene.Instantiate();
			HeartContainer.AddChild(heart);
		}
	}

	public override void _Process(double delta) {
		BossHealthBarFill.Scale = MathHelper.FixedLerp(BossHealthBarFill.Scale, new Vector2(_bossHealthBarValue / _bossHealthBarMax, 1f), 8f, (float)delta);
		BossHealthBar.Modulate = MathHelper.FixedLerp(BossHealthBar.Modulate, _showBossHealthBar ? new Color("#ffffffff") : new Color("#ffffff00"), 8f, (float)delta);
	}

	public static void SetBossHealthBarMax(float max) {
		s_Me._bossHealthBarMax = max;
	}

	public static void UpdateBossHealthBar(float health) {
		s_Me._bossHealthBarValue = health;
	}

	public static void ShowBossHealthBar() {
		s_Me.BossHealthBarFill.Scale = Vector2.One;
		s_Me._showBossHealthBar = true;
	}

	public static void HideBossHealthBar() {
		s_Me.BossHealthBarFill.Scale = Vector2.One;
		s_Me._showBossHealthBar = false;
	}

	public static void UpdateHealth(float health) {
		for (int i = 0; i < 3; i++) {
			TextureRect heart = s_Me.HeartContainer.GetChild<TextureRect>(i);

			if (health >= i + 1) {
				heart.Texture = s_Me.FullHeart;
			} else if (health >= i + 0.5f) {
				heart.Texture = s_Me.HalfHeart;
			} else {
				heart.Texture = s_Me.EmptyHeart;
			}
		}
	}
}
