using Godot;
using Riptide;
using Riptide.Utils;
using System;
using System.Collections.Generic;

public partial class Game : Node2D, Networking.NetworkNode
{
	public enum MessageType
	{
		Strict,
		Unreliable,
		PropogateStrict,
		PropogateUnreliable,
		RelayedStrict,
		RelayedUnreliable,
	}

	private struct UnhandledMessage
	{
		public Message Message;
		public MessageType MessageType;
	}

	public static Game deprecated_Me;

	public static bool IsHost() => s_Server != null;
	public static bool IsOwner(Node node) => node.GetMultiplayerAuthority() == s_Client.Id;
	public static uint Seed;

	private static Game s_Me;
	private static Server s_Server;
	private static Client s_Client;

	[Export] public PackedScene PlayerScene;

	public bool deprecated_IsHost;
	public int[] ClientIds;

	private Networking.RpcMap _rpcMap = new Networking.RpcMap();
	public Networking.RpcMap RpcMap => _rpcMap;

	private ENetMultiplayerPeer _peer;
	private WorldGenerator _worldGenerator;
	private List<UnhandledMessage> _unhandledMessages = new List<UnhandledMessage>();

	public override void _Ready()
	{
		RiptideLogger.Initialize(GD.Print, GD.Print, GD.PushWarning, GD.PushError, false);

		_rpcMap.Register(nameof(StartRpc), StartRpc);

		s_Me = this;

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

		for (int unhandledMessageIndex = 0; unhandledMessageIndex < _unhandledMessages.Count; unhandledMessageIndex++)
		{
			UnhandledMessage unhandledMessage = _unhandledMessages[0];

			_unhandledMessages.RemoveAt(0);

			HandleMessage(unhandledMessage.MessageType, unhandledMessage.Message);
		}
	}

	public static void SendRpcToServer(Node source, string name, MessageSendMode messageSendMode, Action<Message> messageBuilder)
	{
		Message message = Message.Create(messageSendMode, messageSendMode == MessageSendMode.Reliable ? (ushort)MessageType.Strict : (ushort)MessageType.Unreliable);
		message.AddString(source.GetPath());
		message.AddString(name);
		messageBuilder.Invoke(message);

		s_Client.Send(message);
	}

	public static void SendRpcToClients(Node source, string name, MessageSendMode messageSendMode, Action<Message> messageBuilder)
	{
		Message message = Message.Create(messageSendMode, messageSendMode == MessageSendMode.Reliable ? (ushort)MessageType.Strict : (ushort)MessageType.Unreliable);
		message.AddString(source.GetPath());
		message.AddString(name);
		messageBuilder.Invoke(message);

		s_Server.SendToAll(message);
	}

	public static void SendRpcToOtherClients(Node source, string name, MessageSendMode messageSendMode, Action<Message> messageBuilder)
	{
		Message message = Message.Create(messageSendMode, messageSendMode == MessageSendMode.Reliable ? (ushort)MessageType.PropogateStrict : (ushort)MessageType.PropogateUnreliable);
		message.AddInt((int)messageSendMode);
		message.AddString(source.GetPath());
		message.AddString(name);
		messageBuilder.Invoke(message);

		s_Client.Send(message);
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

	private void HandleMessage(MessageType messageType, Message message)
	{
		if (messageType == MessageType.PropogateStrict || messageType == MessageType.PropogateUnreliable)
		{
			MessageSendMode messageSendMode = (MessageSendMode)message.GetInt();
			Message relayMessage = Message.Create(messageSendMode, (ushort)(messageType == MessageType.PropogateStrict ? MessageType.RelayedStrict : MessageType.RelayedUnreliable));

			relayMessage.AddBytes(message.GetBytes(message.UnreadBits / 8));

			s_Server.SendToAll(relayMessage);

			return;
		}

		string path = message.GetString();

		if (path.Length == 0)
		{
			GD.PushWarning("Got empty path??? " + message.ReadBits + " " + message.UnreadBits + " " + messageType + " " + message.SendMode);

			return;
		}

		if (!HasNode(path) || ((messageType == MessageType.Strict || messageType == MessageType.RelayedStrict) && _unhandledMessages.Count > 0))
		{
			GD.PushWarning("Unhandled message for node " + path + " " + path.Length);

			Message unhandledMessage = Message.Create(MessageSendMode.Reliable, (ushort)messageType);
			unhandledMessage.AddString(path);

			unhandledMessage.AddBytes(message.GetBytes(message.UnreadBits / 8));

			_unhandledMessages.Add(new UnhandledMessage
			{
				MessageType = messageType,
				Message = unhandledMessage,
			});

			return;
		}

		Networking.NetworkNode rpcReceiver = GetNode<Networking.NetworkNode>(path);

		string name = message.GetString();

		rpcReceiver.RpcMap.Call(name, message);
	}

	private void MessageRecieved(Object _, MessageReceivedEventArgs eventArguments)
	{
		HandleMessage((MessageType)eventArguments.MessageId, eventArguments.Message);
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
