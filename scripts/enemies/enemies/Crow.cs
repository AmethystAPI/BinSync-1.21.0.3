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
            OnEnter = () => {
                SquashAndStretch.Trigger(new Vector2(0.6f, 1.4f), 8f);
            },
            GetFlock = () => {
                List<Enemy> flock = Crows.Cast<Enemy>().ToList();

                for (int index = 0; index < flock.Count; index++) {
                    if (!IsInstanceValid(flock[index])) {
                        GD.PushWarning("Invalid crow?");

                        flock.RemoveAt(index);

                        index--;
                    }
                }

                return flock;
            }
        });

        _stateMachine.Add(new FlyingAggressive("aggressive", this) {
            OnEnter = () => {
                SquashAndStretch.Trigger(new Vector2(0.6f, 1.4f), 8f);

                Projectile projectile = ProjectileScene.Instantiate<Projectile>();

                projectile.Source = this;

                ProjectileOrigin.AddChild(projectile);

                return projectile;
            }
        });
    }

    protected override void ActivateRpc(Message message) {
        base.ActivateRpc(message);

        if (!IsInstanceValid(this)) {
            GD.PushError("Trying to activate invalid crow!");

            return;
        }

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
