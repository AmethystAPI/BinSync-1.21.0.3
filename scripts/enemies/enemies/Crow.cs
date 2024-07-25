using System.Collections.Generic;
using System.Linq;
using Godot;
using Riptide;

public partial class Crow : Enemy {
    [Export] public PackedScene ProjectileScene;
    [Export] public Node2D ProjectileOrigin;

    public override void AddStates() {
        base.AddStates();

        _stateMachine.Add(new FlockScared("scared", this) {
            OnEnter = () => {
                SquashAndStretch.Trigger(new Vector2(0.6f, 1.4f), 8f);
            },
            GetFlock = () => {
                Node parent = GetParent();

                List<Enemy> flock = GetTree().GetNodesInGroup("Enemies").Where(node => node.GetParent() == parent && node is Crow).Cast<Enemy>().ToList();
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

    protected override string GetDefaultState() {
        return "scared";
    }
}
