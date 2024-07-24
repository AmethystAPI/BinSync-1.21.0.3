using System.Collections.Generic;
using System.Linq;
using Godot;
using Riptide;

public partial class Crow : Enemy {
    public static List<Crow> Crows = new List<Crow>();

    [Export] public PackedScene ProjectileScene;
    [Export] public Node2D ProjectileOrigin;

    public override void AddStates() {
        base.AddStates();

        _stateMachine.Add(new FlockScared("scared", this) {
            GetFlock = () => Crows.Cast<Enemy>().ToList()
        });

        _stateMachine.Add(new FlyingAggressive("aggressive", this) {
            OnEnter = () => {
                Projectile projectile = ProjectileScene.Instantiate<Projectile>();

                projectile.Source = this;

                ProjectileOrigin.AddChild(projectile);

                return projectile;
            }
        });
    }

    protected override void ActivateRpc(Message message) {
        base.ActivateRpc(message);

        Crows.Add(this);
    }

    protected override void DamageRpc(Message message) {
        base.DamageRpc(message);

        if (Dead && Crows.Contains(this)) Crows.Remove(this);
    }

    public override void _Notification(int what) {
        if (what == NotificationPredelete && Crows.Contains(this)) Crows.Remove(this);
    }

    protected override string GetDefaultState() {
        return "scared";
    }
}
