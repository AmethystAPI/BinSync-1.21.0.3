using Godot;
using System;

public partial class Game : Node2D
{
	private ENetMultiplayerPeer _peer;

	public override void _Ready()
	{
		Multiplayer.PeerConnected += (id) => GD.Print("Network peer connected " + id);
		Multiplayer.ConnectedToServer += () => GD.Print("Connected to server");
		Multiplayer.ConnectionFailed += () => GD.Print("Connection failed");

		if (!Host()) Join("127.0.0.1");
	}

	public bool Host()
	{
		GD.Print("Hosting...");

		_peer = new ENetMultiplayerPeer();

		Error error = _peer.CreateServer(25566, 8);

		if (error != Error.Ok) return false;

		_peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

		Multiplayer.MultiplayerPeer = _peer;

		return true;
	}

	public bool Join(string address)
	{
		GD.Print("Joining...");

		_peer = new ENetMultiplayerPeer();

		_peer.CreateClient(address, 25566);

		_peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

		Multiplayer.MultiplayerPeer = _peer;

		return true;
	}

	private void OnPeerConnected(long id)
	{
		GD.Print("Peer connected " + id);
	}
}
