using System;
using System.Collections.Generic;
using Godot;
using Riptide;

namespace Networking {
  public class NetworkPoint {
    public bool IsOwner => _source.GetMultiplayerAuthority() == NetworkManager.LocalClient.Id;
    public string Path => _source.GetPath();

    private Dictionary<string, Action<Message>> _registeredMessageHandlers = new Dictionary<string, Action<Message>>();
    private NetworkPointUser _source;

    public void Setup(NetworkPointUser source) {
      _source = source;
    }

    public void Register(string name, Action<Message> messageHandler) {
      _registeredMessageHandlers.Add(name, messageHandler);
    }

    public void Register<ValueType>(string name, NetworkedVariable<ValueType> syncedVariable) {
      syncedVariable.Register(_source, name);

      _registeredMessageHandlers.Add(name, syncedVariable.ReceiveUpdate);
    }

    public void HandleMessage(string name, Message message) {
      _registeredMessageHandlers[name].Invoke(message);
    }

    public void SendRpcToServer(string name, Action<Message> messageBuilder = null, MessageSendMode messageSendMode = MessageSendMode.Reliable) {
      NetworkManager.SendRpcToServer(_source, name, messageBuilder, messageSendMode);
    }

    public void SendRpcToClients(string name, Action<Message> messageBuilder = null, MessageSendMode messageSendMode = MessageSendMode.Reliable) {
      NetworkManager.SendRpcToClients(_source, name, messageBuilder, messageSendMode);
    }

    public void SendRpcToClientsFast(string name, Action<Message> messageBuilder = null, MessageSendMode messageSendMode = MessageSendMode.Reliable) {
      NetworkManager.SendRpcToClientsFast(_source, name, messageBuilder, messageSendMode);
    }

    public void BounceRpcToClients(string name, Action<Message> messageBuilder = null, MessageSendMode messageSendMode = MessageSendMode.Reliable) {
      NetworkManager.BounceRpcToClients(_source, name, messageBuilder, messageSendMode);
    }

    public void BounceRpcToClientsFast(string name, Action<Message> messageBuilder = null, MessageSendMode messageSendMode = MessageSendMode.Reliable) {
      NetworkManager.BounceRpcToClientsFast(_source, name, messageBuilder, messageSendMode);
    }
  }

  public interface NetworkPointUser {
    public NetworkPoint NetworkPoint { get; set; }
    public int GetMultiplayerAuthority();
    public NodePath GetPath();
  }
}