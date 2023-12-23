using Godot;
using System;
using System.Collections.Generic;

public partial class Game : Node2D
{
	public static Game Me;

	[Export] public PackedScene PlayerScene;
	[Export] public PackedScene RoomScene;

	public bool IsHost;

	private ENetMultiplayerPeer _peer;

	public override void _Ready()
	{
		Me = this;

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

		GenerateRooms();

		Rpc(nameof(StartRpc), peers.ToArray());
	}

	private void GenerateRooms()
	{
		for (int i = 0; i < 5; i++)
		{
			Rpc(nameof(SpawnRoomRpc), Vector2.Right * 16 * 10 * i);
		}
	}

	[Rpc(CallLocal = true)]
	private void SpawnRoomRpc(Vector2 position)
	{
		Node2D room = RoomScene.Instantiate<Node2D>();

		room.Position = position;

		AddChild(room);
	}

	[Rpc(CallLocal = true)]
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
