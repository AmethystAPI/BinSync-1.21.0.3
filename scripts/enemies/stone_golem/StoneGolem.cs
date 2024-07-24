using Networking;
using Riptide;

public partial class StoneGolem : Enemy {
    public override void _Ready() {
        base._Ready();

        NetworkPoint.Register(nameof(SetRandomSeedRpc), SetRandomSeedRpc);

        if (NetworkManager.IsHost) NetworkPoint.SendRpcToClients(nameof(SetRandomSeedRpc), message => message.AddULong(_nodeStateMachine.GetNode<StoneGolemRoll>("Roll").Random.Seed));
    }

    public override void SyncPosition(float delta) {
        if (NetworkPoint.IsOwner) {
            _networkedPosition.Value = GlobalPosition;
        } else if (_stateMachine.CurrentState != "roll") {
            if (_networkedPosition.Value.DistanceSquaredTo(GlobalPosition) > 64) GlobalPosition = _networkedPosition.Value;

            GlobalPosition = GlobalPosition.Lerp(_networkedPosition.Value, delta * 20.0f);
        }
    }

    private void SetRandomSeedRpc(Message message) {
        _stateMachine.GetNode<StoneGolemRoll>("roll").Random.Seed = message.GetULong();
    }
}
