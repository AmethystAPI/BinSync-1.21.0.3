using Godot;
using Riptide;
using Riptide.Utils;
using System;
using System.Collections.Generic;

public partial class Game : Node2D, Networking.NetworkNode
{
	public static bool IsHost() => Server != null;
	public static bool IsOwner(Node node) => node.GetMultiplayerAuthority() == s_Client.Id;
	public static uint Seed;
	public static Server Server;

	private static Game s_Me;
	private static Client s_Client;

	[Export] public PackedScene PlayerScene;

	public int[] ClientIds;

	private Networking.RpcMap _rpcMap = new Networking.RpcMap();
	public Networking.RpcMap RpcMap => _rpcMap;

	private ENetMultiplayerPeer _peer;
	private WorldGenerator _worldGenerator;
	private List<Message> _unhandledMessages = new List<Message>();

	public override void _Ready()
	{
		RiptideLogger.Initialize(GD.Print, GD.Print, GD.PushWarning, GD.PushError, false);

		_rpcMap.Register(nameof(StartRpc), StartRpc);
		_rpcMap.Register(nameof(CleanupRpc), CleanupRpc);

		s_Me = this;

		_worldGenerator = GetNode<WorldGenerator>("WorldGenerator");

		// if (!Host()) Join("127.0.0.1");
	}

	// public override void _Input(InputEvent @event)
	// {
	// 	if (@event.IsActionPressed("host"))
	// 	{
	// 		Host();
	// 	}

	// 	if (@event.IsActionPressed("join"))
	// 	{
	// 		Join("104.33.194.150");
	// 		// Join("127.0.0.1");
	// 	}
	// }

	public override void _PhysicsProcess(double delta)
	{
		if (Server != null) Server.Update();
		if (s_Client != null) s_Client.Update();

		for (int unhandledMessageIndex = 0; unhandledMessageIndex < _unhandledMessages.Count; unhandledMessageIndex++)
		{
			Message unhandledMessage = _unhandledMessages[0];

			_unhandledMessages.RemoveAt(0);

			HandleMessage(unhandledMessage);
		}
	}

	public static void SendRpcToServer(Node source, string name, MessageSendMode messageSendMode, Action<Message> messageBuilder)
	{
		Message message = Message.Create(messageSendMode, 0);
		message.AddString(name);
		message.AddString(source.GetPath());
		messageBuilder.Invoke(message);

		s_Client.Send(message);
	}

	public static void SendRpcToClients(Node source, string name, MessageSendMode messageSendMode, Action<Message> messageBuilder)
	{
		Message message = Message.Create(messageSendMode, 0);
		message.AddString(name);
		message.AddString(source.GetPath());
		messageBuilder.Invoke(message);

		Server.SendToAll(message);
	}

	public static void SendRpcToAllClients(Node source, string name, MessageSendMode messageSendMode, Action<Message> messageBuilder)
	{
		Message message = Message.Create(messageSendMode, 1);
		message.AddString(name);
		message.AddString(source.GetPath());
		messageBuilder.Invoke(message);

		s_Client.Send(message);
	}

	public static bool Host()
	{
		GD.Print("Hosting...");

		Server = new Server(new Riptide.Transports.Tcp.TcpServer());

		try
		{
			Server.Start(25566, 2, 0, false);
		}
		catch
		{
			Server = null;

			return false;
		}

		Server.MessageReceived += s_Me.MessageRecieved;

		GD.Print("Successfully started server, starting client!");

		Join("127.0.0.1");

		return true;
	}

	public static bool Join(string address)
	{
		GD.Print("Joining...");

		s_Client = new Client(new Riptide.Transports.Tcp.TcpClient());
		s_Client.Connect(address + ":25566", 5, 0, null, false);

		s_Client.MessageReceived += s_Me.MessageRecieved;

		return true;
	}

	public static void Start()
	{
		List<int> clientIds = new List<int>();

		foreach (Connection connection in Server.Clients)
		{
			clientIds.Add(connection.Id);
		}

		SendRpcToClients(s_Me, nameof(StartRpc), MessageSendMode.Reliable, message =>
		{
			message.AddInts(clientIds.ToArray());
			message.AddUInt(new RandomNumberGenerator().Randi());
		});
	}

	public static void Restart()
	{
		SendRpcToClients(s_Me, nameof(CleanupRpc), MessageSendMode.Reliable, message => { });

		List<int> clientIds = new List<int>();

		foreach (Connection connection in Server.Clients)
		{
			clientIds.Add(connection.Id);
		}

		SendRpcToClients(s_Me, nameof(StartRpc), MessageSendMode.Reliable, message =>
		{
			message.AddInts(clientIds.ToArray());
			message.AddUInt(new RandomNumberGenerator().Randi());
		});
	}

	private void HandleMessage(Message message)
	{
		string name = message.GetString();

		string path = message.GetString();

		// if (message.SendMode == MessageSendMode.Reliable) GD.PushWarning("Handling " + name + " on " + path);

		if (!HasNode(path))
		{
			if (message.SendMode == MessageSendMode.Unreliable) return;

			GD.PushWarning("Unhandled message " + name + " for node " + path + " " + path.Length);

			Message unhandledMessage = Message.Create(message.SendMode, 0);

			unhandledMessage.AddString(name);
			unhandledMessage.AddString(path);

			while (message.UnreadBits > 0)
			{
				int bitsToWrite = Math.Min(message.UnreadBits, 8);

				byte bits;

				message.GetBits(bitsToWrite, out bits);

				unhandledMessage.AddBits(bits, bitsToWrite);
			}

			_unhandledMessages.Add(unhandledMessage);

			return;
		}

		Networking.NetworkNode rpcReceiver = GetNode<Networking.NetworkNode>(path);

		rpcReceiver.RpcMap.Call(name, message);
	}

	private void MessageRecieved(Object _, MessageReceivedEventArgs eventArguments)
	{
		if (eventArguments.MessageId == 1)
		{
			Message relayMessage = Message.Create(eventArguments.Message.SendMode, 0);

			while (eventArguments.Message.UnreadBits > 0)
			{
				int bitsToWrite = Math.Min(eventArguments.Message.UnreadBits, 8);

				byte bits;

				eventArguments.Message.GetBits(bitsToWrite, out bits);

				relayMessage.AddBits(bits, bitsToWrite);
			}

			Server.SendToAll(relayMessage);

			return;
		}

		HandleMessage(eventArguments.Message);
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

	private void CleanupRpc(Message message)
	{
		while (Player.Players.Count > 0)
		{
			Player.Players[0].Cleanup();
		}

		_worldGenerator.Cleanup();
	}
}
