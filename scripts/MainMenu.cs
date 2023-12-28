using Godot;
using Networking;

public partial class MainMenu : Control
{
	public void Host()
	{
		NetworkManager.Host();

		QueueFree();
	}

	public void Join()
	{
		// NetworkManager.Join("127.0.0.1");
		NetworkManager.Join("104.33.194.150");

		QueueFree();
	}
}
