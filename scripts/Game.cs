using Godot;
using System;
using System.Collections.Generic;

public partial class Game : Node2D
{
	public static Game Me;

	[Export] public PackedScene PlayerScene;

	public bool IsHost;
	public int Seed;

	private ENetMultiplayerPeer _peer;
	private WorldGenerator _worldGenerator;

	public override void _Ready()
	{
		Me = this;

		Multiplayer.PeerConnected += (id) => OnPeerConnected(id);

		_worldGenerator = GetNode<WorldGenerator>("WorldGenerator");

		if (!Host()) Join("127.0.0.1");

		List<int> peers = new List<int>(Multiplayer.GetPeers())
		{
			1
		};

		Rpc(nameof(StartRpc), peers.ToArray(), new RandomNumberGenerator().Randi());
	}

	public bool Host()
	{
		GD.Print("Hosting...");

		_peer = new ENetMultiplayerPeer();

		Error error = _peer.CreateServer(25566, 8);

		if (error != Error.Ok) return false;

		_peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

		Multiplayer.MultiplayerPeer = _peer;

		IsHost = true;

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

		if (!IsHost) return;

		List<int> peers = new List<int>(Multiplayer.GetPeers())
		{
			1
		};

		Rpc(nameof(StartRpc), peers.ToArray(), new RandomNumberGenerator().Randi());
	}

	[Rpc(CallLocal = true)]
	private void StartRpc(int[] peers, int seed)
	{
		Seed = seed;

		_worldGenerator.Start();

		foreach (int peerId in peers)
		{
			Node2D player = PlayerScene.Instantiate<Node2D>();

			player.SetMultiplayerAuthority(peerId, true);

			AddChild(player);
		}
	}
}
