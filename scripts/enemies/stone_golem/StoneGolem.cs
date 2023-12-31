public partial class StoneGolem : Enemy
{
    public override bool CanDamage(Projectile projectile)
    {
        if (!base.CanDamage(projectile)) return false;

        if (_stateMachine.CurrentState == "Roll") return false;

        if (_stateMachine.CurrentState == "Hurt") return false;

        return true;
    }

    public override void SyncPosition(float delta)
    {
        if (NetworkPoint.IsOwner)
        {
            _networkedPosition.Value = GlobalPosition;
        }
        else if (_stateMachine.CurrentState != "Roll")
        {
            if (_networkedPosition.Value.DistanceSquaredTo(GlobalPosition) > 64) GlobalPosition = _networkedPosition.Value;

            GlobalPosition = GlobalPosition.Lerp(_networkedPosition.Value, delta * 20.0f);
        }
    }
}
