using System;
using System.Collections.Generic;
using Godot;
using Riptide;
using Riptide.Transports.Steam;
using Riptide.Utils;

namespace Networking {
  public partial class NetworkManager : Node {
    public static Server LocalServer;
    public static Client LocalClient;
    public static bool IsHost => LocalServer != null;
    public static Action<ServerConnectedEventArgs> ClientConnected;

    private static NetworkManager s_Me;
    private static SteamServer s_LocalSteamServer;

    private Dictionary<string, int> _nameIndexes = new Dictionary<string, int>();

    public override void _Ready() {
      s_Me = this;

      RiptideLogger.Initialize(GD.Print, GD.Print, GD.PushWarning, GD.PushError, false);
    }

    public override void _PhysicsProcess(double delta) {
      if (LocalServer != null) LocalServer.Update();
      if (LocalClient != null) LocalClient.Update();
    }

    public static NodeType SpawnNetworkSafe<NodeType>(PackedScene packedScene, string baseName, int authority = 1) where NodeType : Node {
      NodeType node = packedScene.Instantiate<NodeType>();

      if (!s_Me._nameIndexes.ContainsKey(baseName)) s_Me._nameIndexes.Add(baseName, 0);

      node.Name = baseName + " " + s_Me._nameIndexes[baseName];

      node.SetMultiplayerAuthority(authority);

      s_Me._nameIndexes[baseName]++;

      return node;
    }

    public static void SendRpcToServer(NetworkPointUser source, string name, Action<Message> messageBuilder = null, MessageSendMode messageSendMode = MessageSendMode.Reliable) {
      if (!IsInstanceValid(source as Node)) GD.PushError("Trying to Send RPC From Invalid Instance " + name);

      Message message = Message.Create(messageSendMode, 0);
      message.AddString(name);
      message.AddString(source.GetPath());

      messageBuilder?.Invoke(message);

      LocalClient.Send(message);
    }

    public static void SendRpcToClients(NetworkPointUser source, string name, Action<Message> messageBuilder = null, MessageSendMode messageSendMode = MessageSendMode.Reliable) {
      if (!IsInstanceValid(source as Node)) GD.PushError("Trying to Send RPC From Invalid Instance " + name);

      Message message = Message.Create(messageSendMode, 0);
      message.AddString(name);
      message.AddString(source.GetPath());

      messageBuilder?.Invoke(message);

      LocalServer.SendToAll(message);
    }

    public static void SendRpcToClientsFast(NetworkPointUser source, string name, Action<Message> messageBuilder = null, MessageSendMode messageSendMode = MessageSendMode.Reliable) {
      if (!IsInstanceValid(source as Node)) GD.PushError("Trying to Send RPC From Invalid Instance " + name);

      Message message = Message.Create(messageSendMode, 0);

      int initialBits = message.WrittenBits;

      message.AddString(name);
      message.AddString(source.GetPath());

      messageBuilder?.Invoke(message);

      Message localMessage = Message.Create();
      int bitsToRead = message.WrittenBits - initialBits;
      int readPosition = initialBits;

      while (bitsToRead > 0) {
        int bitsToWrite = Math.Min(bitsToRead, 8);

        byte bits;

        message.PeekBits(bitsToWrite, readPosition, out bits);

        localMessage.AddBits(bits, bitsToWrite);

        readPosition += 8;
        bitsToRead -= bitsToWrite;
      }

      foreach (Connection client in LocalServer.Clients) {
        if (client.Id == LocalClient.Id) continue;

        LocalServer.Send(message, client.Id);
      }

      s_Me.HandleMessage(localMessage);
    }

    public static void BounceRpcToClients(NetworkPointUser source, string name, Action<Message> messageBuilder = null, MessageSendMode messageSendMode = MessageSendMode.Reliable) {
      if (!IsInstanceValid(source as Node)) GD.PushError("Trying to Send RPC From Invalid Instance " + name);

      Message message = Message.Create(messageSendMode, 1);

      int initialBits = message.WrittenBits;

      message.AddString(name);
      message.AddString(source.GetPath());

      messageBuilder?.Invoke(message);

      LocalClient.Send(message);
    }

    public static void BounceRpcToClientsFast(NetworkPointUser source, string name, Action<Message> messageBuilder = null, MessageSendMode messageSendMode = MessageSendMode.Reliable) {
      if (!IsInstanceValid(source as Node)) GD.PushError("Trying to Send RPC From Invalid Instance " + name);

      Message message = Message.Create(messageSendMode, 2);

      int initialBits = message.WrittenBits;

      message.AddString(name);
      message.AddString(source.GetPath());

      messageBuilder?.Invoke(message);

      Message localMessage = Message.Create();
      int bitsToRead = message.WrittenBits - initialBits;
      int readPosition = initialBits;

      while (bitsToRead > 0) {
        int bitsToWrite = Math.Min(bitsToRead, 8);

        byte bits;

        message.PeekBits(bitsToWrite, readPosition, out bits);

        localMessage.AddBits(bits, bitsToWrite);

        readPosition += 8;
        bitsToRead -= bitsToWrite;
      }

      LocalClient.Send(message);

      s_Me.HandleMessage(localMessage);
    }

    public static bool Host() {
      s_LocalSteamServer = new SteamServer();
      LocalServer = new Server(s_LocalSteamServer);

      try {
        LocalServer.Start(25566, 32, 0, false);
      } catch {
        LocalServer = null;

        return false;
      }

      LocalServer.MessageReceived += s_Me.OnMessageRecieved;

      LocalServer.ClientConnected += s_Me.OnClientConnected;

      LocalClient = new Client(new SteamClient(s_LocalSteamServer));
      LocalClient.Connect("localhost", 5, 0, null, false);

      LocalClient.MessageReceived += s_Me.OnMessageRecieved;

      return true;
    }

    public static bool Join(ulong serverId) {
      LocalClient = new Client(new SteamClient());

      LocalClient.Connect(serverId.ToString(), 5, 0, null, false);

      LocalClient.MessageReceived += s_Me.OnMessageRecieved;

      return true;
    }

    public static bool IsOwner(Node node) {
      return node.GetMultiplayerAuthority() == LocalClient.Id;
    }

    private void HandleMessage(Message message) {
      string name = message.GetString();

      string path = message.GetString();

      if (!HasNode(path)) {
        if (message.SendMode == MessageSendMode.Reliable) GD.PushWarning("Ignoring Reliable Rpc " + name + " for node " + path + " because the node does not exist!");

        return;
      }

      GetNode<NetworkPointUser>(path).NetworkPoint.HandleMessage(name, message);
    }

    private void OnMessageRecieved(object _, MessageReceivedEventArgs eventArguments) {
      if (eventArguments.MessageId == 1 || eventArguments.MessageId == 2) {
        Message relayMessage = Message.Create(eventArguments.Message.SendMode, 0);

        while (eventArguments.Message.UnreadBits > 0) {
          int bitsToWrite = Math.Min(eventArguments.Message.UnreadBits, 8);

          byte bits;

          eventArguments.Message.GetBits(bitsToWrite, out bits);

          relayMessage.AddBits(bits, bitsToWrite);
        }

        if (eventArguments.MessageId == 1) {
          LocalServer.SendToAll(relayMessage);
        } else {
          foreach (Connection connection in LocalServer.Clients) {
            if (connection == eventArguments.FromConnection) continue;

            LocalServer.Send(relayMessage, connection.Id);
          }
        }

        return;
      }

      HandleMessage(eventArguments.Message);
    }

    private void OnClientConnected(object server, ServerConnectedEventArgs eventArguments) {
      ClientConnected?.Invoke(eventArguments);
    }
  }
}