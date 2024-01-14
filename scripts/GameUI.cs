using Godot;

public partial class GameUI : Control {
	private static GameUI s_Me;

	[Export] public PackedScene HeartScene;
	[Export] public HBoxContainer HeartContainer;
	[Export] public Texture2D FullHeart;
	[Export] public Texture2D HalfHeart;
	[Export] public Texture2D EmptyHeart;

	public override void _Ready() {
		s_Me = this;

		for (int i = 0; i < 3; i++) {
			Node heart = HeartScene.Instantiate();
			HeartContainer.AddChild(heart);
		}
	}

	public static void UpdateHealth(float health) {
		for (int i = 0; i < 3; i++) {
			TextureRect heart = s_Me.HeartContainer.GetChild<TextureRect>(i);

			if (health >= i + 1) {
				heart.Texture = s_Me.FullHeart;
			} else if (health > i) {
				heart.Texture = s_Me.HalfHeart;
			} else {
				heart.Texture = s_Me.EmptyHeart;
			}
		}
	}
}
