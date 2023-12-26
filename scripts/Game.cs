using Godot;
using Riptide;
using Riptide.Utils;
using System;
using System.Collections.Generic;

public partial class Game : Node2D, Networking.NetworkNode
{
	public static Game deprecated_Me;

	public static bool IsHost() => s_Server != null;
	public static bool IsOwner(Node node) => node.GetMultiplayerAuthority() == s_Client.Id;

	private static Game s_Me;
	private static Server s_Server;
	private static Client s_Client;

	[Export] public PackedScene PlayerScene;

	public bool deprecated_IsHost;
	public int[] ClientIds;
	public uint Seed;

	private Networking.RpcMap _rpcMap = new Networking.RpcMap();
	public Networking.RpcMap RpcMap => _rpcMap;

	private ENetMultiplayerPeer _peer;
	private WorldGenerator _worldGenerator;

	public override void _Ready()
	{
		RiptideLogger.Initialize(GD.Print, GD.Print, GD.PushWarning, GD.PushError, false);

		_rpcMap.Register(nameof(StartRpc), StartRpc);

		s_Me = this;
		deprecated_Me = this;

		_worldGenerator = GetNode<WorldGenerator>("WorldGenerator");

		if (!Host()) Join("127.0.0.1");

		// List<int> peers = new List<int>(Multiplayer.GetPeers())
		// {
		// 	1
		// };

		// Rpc(nameof(StartRpc), peers.ToArray(), new RandomNumberGenerator().Randi());
	}

	public override void _PhysicsProcess(double delta)
	{
		if (s_Server != null) s_Server.Update();
		if (s_Client != null) s_Client.Update();
	}

	public static void SendRpcToServer(Node source, string name, MessageSendMode messageSendMode, Action<Message> messageBuilder)
	{
		Message message = Message.Create(messageSendMode, 0);
		message.AddString(source.GetPath());
		message.AddString(name);
		messageBuilder.Invoke(message);

		s_Client.Send(message);
	}

	public static void SendRpcToClients(Node source, string name, MessageSendMode messageSendMode, Action<Message> messageBuilder)
	{
		Message message = Message.Create(messageSendMode, 0);
		message.AddString(source.GetPath());
		message.AddString(name);
		messageBuilder.Invoke(message);

		s_Server.SendToAll(message);
	}

	public static void SendRpcToClientsExcept(Node source, string name, MessageSendMode messageSendMode, Action<Message> messageBuilder, ushort clientId)
	{
		Message message = Message.Create(messageSendMode, 0);
		message.AddString(source.GetPath());
		message.AddString(name);
		messageBuilder.Invoke(message);

		s_Server.SendToAll(message, clientId);
	}

	public bool Host()
	{
		GD.Print("Hosting...");

		s_Server = new Server();

		try
		{
			s_Server.Start(25566, 2, 0, false);
		}
		catch
		{
			s_Server = null;

			return false;
		}

		s_Server.MessageReceived += MessageRecieved;
		s_Server.ClientConnected += ClientConnected;

		GD.Print("Successfully started server, starting client!");

		Join("127.0.0.1");

		deprecated_IsHost = true;

		return true;
	}

	public bool Join(string address)
	{
		GD.Print("Joining...");

		s_Client = new Client();
		s_Client.Connect(address + ":25566", 5, 0, null, false);

		s_Client.MessageReceived += MessageRecieved;

		return true;
	}

	private void MessageRecieved(Object _, MessageReceivedEventArgs eventArguments)
	{
		string path = eventArguments.Message.GetString();

		Networking.NetworkNode rpcReceiver = GetNode<Networking.NetworkNode>(path);

		string name = eventArguments.Message.GetString();

		rpcReceiver.RpcMap.Call(name, eventArguments.Message);
	}

	private void ClientConnected(Object _, ServerConnectedEventArgs eventArguments)
	{
		if (s_Server.ClientCount != 2 || eventArguments.Client != s_Server.Clients[1]) return;

		if (s_Server.ClientCount != 2) return;

		List<int> clientIds = new List<int>();

		foreach (Connection connection in s_Server.Clients)
		{
			clientIds.Add(connection.Id);
		}

		SendRpcToClients(this, nameof(StartRpc), MessageSendMode.Reliable, message =>
		{
			message.AddInts(clientIds.ToArray());
			message.AddUInt(new RandomNumberGenerator().Randi());
		});
	}

	private void StartRpc(Message message)
	{
		ClientIds = message.GetInts();
		Seed = message.GetUInt();

		_worldGenerator.Start();

		foreach (int clientId in ClientIds)
		{
			Player player = PlayerScene.Instantiate<Player>();

			player.SetMultiplayerAuthority(clientId, true);

			AddChild(player);
		}
	}
}
