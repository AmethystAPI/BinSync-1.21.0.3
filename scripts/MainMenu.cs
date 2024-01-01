using Godot;
using Networking;

public partial class MainMenu : Control
{
	[Export] public TextEdit ipAdressInput;
	[Export] public Button hostButton;
	[Export] public Button joinButton;
	[Export] public Button startButton;

	public void Host()
	{
		NetworkManager.Host();

		hostButton.QueueFree();
		joinButton.QueueFree();
	}

	public void Join()
	{
		// NetworkManager.Join("127.0.0.1");
		// NetworkManager.Join("104.33.194.150");

		NetworkManager.Join(ipAdressInput.Text);

		QueueFree();
	}

	public void Start()
	{
		if (!NetworkManager.IsHost) return;

		Game.Start();

		QueueFree();
	}
}
