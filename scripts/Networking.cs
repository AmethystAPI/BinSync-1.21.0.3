using System;
using System.Collections.Generic;
using Godot;
using Riptide;

public class Networking
{
  public class RpcMap
  {
    private Dictionary<string, Action<Message>> _definitions = new Dictionary<string, Action<Message>>();

    public void Register(string name, Action<Message> action)
    {
      _definitions.Add(name, action);
    }

    public void Register<ValueType>(SyncedVariable<ValueType> syncedVariable, Node source)
    {
      syncedVariable.Register(source);

      _definitions.Add(syncedVariable.Name, syncedVariable.ReceiveUpdate);
    }

    public void Call(string name, Message message)
    {
      _definitions[name].Invoke(message);
    }
  }

  public enum Authority
  {
    Server,
    Client
  }

  public class SyncedVariable<ValueType>
  {
    public string Name;

    private ValueType _value;
    public ValueType Value
    {
      get
      {
        return _value;
      }

      set
      {
        if (_autoSynchronize && !_value.Equals(value)) SendUpdate();

        _value = value;
      }
    }

    private Node _source;
    private Authority _authority;
    private bool _autoSynchronize;
    private int _lastRecievedIndex = -1;
    private int _lastSentIndex = -1;
    private ulong _lastSendTick = 0;
    private ulong _minimumSendDelay = 50;

    public SyncedVariable(string name, ValueType intitalValue, Authority authority, bool autoSynchronize = false, ulong minimumSendDelay = 50)
    {
      Name = name;
      _value = intitalValue;
      _authority = authority;
      _autoSynchronize = autoSynchronize;
      _minimumSendDelay = minimumSendDelay;
    }

    public void Register(Node source)
    {
      _source = source;
    }

    public void Sync()
    {
      SendUpdate();
    }

    public void ReceiveUpdate(Message message)
    {
      bool propogate = message.GetBool();

      int index = message.GetInt();

      if (propogate)
      {
        if (!Game.IsOwner(_source)) GD.PushWarning("Propogating " + _source.Name + " " + index + " " + _lastRecievedIndex);
      }
      else
      {
        if (!Game.IsHost() && !Game.IsOwner(_source)) GD.PushWarning("Recieved update " + _source.Name + " " + index + " " + _lastRecievedIndex);
      }

      if (index <= _lastRecievedIndex) return;

      _lastRecievedIndex = index;

      if (typeof(ValueType) == typeof(int))
      {
        _value = (ValueType)(object)message.GetInt();
      }

      if (typeof(ValueType) == typeof(float))
      {
        _value = (ValueType)(object)message.GetFloat();
      }

      if (typeof(ValueType) == typeof(Vector2))
      {
        _value = (ValueType)(object)new Vector2(message.GetFloat(), message.GetFloat());
      }

      if (propogate) Game.SendRpcToClients(_source, Name, _autoSynchronize ? MessageSendMode.Reliable : MessageSendMode.Unreliable, SetupMessage(false));
    }

    private Action<Message> SetupMessage(bool propogate)
    {
      return (Message message) =>
      {
        message.AddBool(propogate);

        _lastSentIndex++;
        message.AddInt(_lastSentIndex);

        if (typeof(ValueType) == typeof(int))
        {
          message.AddInt((int)(object)_value);
        }

        if (typeof(ValueType) == typeof(float))
        {
          message.AddFloat((float)(object)_value);
        }

        if (typeof(ValueType) == typeof(Vector2))
        {
          Vector2 castedValue = (Vector2)(object)_value;
          message.AddFloat(castedValue.X);
          message.AddFloat(castedValue.Y);
        }
      };
    }

    private void SendUpdate()
    {
      if (_source == null) GD.PushError("Can not send updates for a synched variable that has not been registered!");

      if (!Game.IsOwner(_source)) return;

      ulong now = Time.GetTicksMsec();

      if (now - _lastSendTick < _minimumSendDelay) return;

      _lastSendTick = now;

      if (_authority == Authority.Server)
      {
        Game.SendRpcToClients(_source, Name, _autoSynchronize ? MessageSendMode.Reliable : MessageSendMode.Unreliable, SetupMessage(false));
      }

      if (_authority == Authority.Client)
      {
        Game.SendRpcToServer(_source, Name, _autoSynchronize ? MessageSendMode.Reliable : MessageSendMode.Unreliable, SetupMessage(true));
      }
    }
  }

  public interface NetworkNode
  {
    public RpcMap RpcMap { get; }
  }
}