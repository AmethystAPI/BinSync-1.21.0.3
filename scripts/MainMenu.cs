using Godot;
using Riptide;
using System;

public partial class MainMenu : Control
{
	public void Host()
	{
		Game.Host();

		Game.Server.ClientConnected += ClientConnected;

		QueueFree();
	}

	public void Join()
	{
		// Game.Join("127.0.0.1");
		Game.Join("104.33.194.150");

		QueueFree();
	}

	private void ClientConnected(Object _, ServerConnectedEventArgs eventArguments)
	{
		if (Game.Server.ClientCount != 2 || eventArguments.Client != Game.Server.Clients[1]) return;

		if (Game.Server.ClientCount != 2) return;

		Game.Start();
	}
}
