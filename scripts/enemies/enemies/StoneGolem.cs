using Networking;
using Riptide;

public partial class StoneGolem : Enemy {
    public override void _Ready() {
        base._Ready();

        NetworkPoint.Register(nameof(SetRandomSeedRpc), SetRandomSeedRpc);
    }

    public override void AddStates() {
        base.AddStates();

        _stateMachine.Add(new Idle("idle", this));
        _stateMachine.Add(new Roll("idle", this));

        if (NetworkManager.IsHost) NetworkPoint.SendRpcToClients(nameof(SetRandomSeedRpc), message => message.AddULong(_stateMachine.GetState<Roll>("attack").Random.Seed));
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
        _stateMachine.GetState<Roll>("roll").Random.Seed = message.GetULong();
    }
}
