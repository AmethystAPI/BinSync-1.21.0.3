using Godot;
using System;
using System.Collections.Generic;

public partial class Game : Node2D
{
	[Export] public PackedScene PlayerScene;

	private ENetMultiplayerPeer _peer;
	private bool _host;

	public override void _Ready()
	{
		Multiplayer.PeerConnected += (id) => OnPeerConnected(id);

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

		_host = true;

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

		if (!_host) return;

		List<int> peers = new List<int>(Multiplayer.GetPeers())
		{
			1
		};

		Rpc(nameof(StartRpc), peers.ToArray());
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	private void StartRpc(int[] peers)
	{
		foreach (int peerId in peers)
		{
			Node2D player = PlayerScene.Instantiate<Node2D>();

			AddChild(player);

			player.GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(peerId);
		}
	}
}
