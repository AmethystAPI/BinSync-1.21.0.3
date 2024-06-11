using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Interactables : Node {
    private static List<Interactable> s_ActiveInteractables = new List<Interactable>();
    private static Interactables s_Me;

    public override void _Ready() {
        s_Me = this;

        GetTree().ProcessFrame += InactivateInteractables;
    }

    private void InactivateInteractables() {
        Godot.Collections.Array<Node> nodes = GetTree().GetNodesInGroup("Interactables");

        s_ActiveInteractables.Clear();

        foreach (Node node in nodes) {
            if (!(node is Interactable interactable)) continue;

            interactable.InactivateInteract();
        }
    }

    public static void ActivateClosest(Node2D interactor) {
        Interactable interactable = GetClosest(interactor);

        if (interactable == null) return;

        interactable.ActivateInteract();
        s_ActiveInteractables.Add(interactable);
    }

    public static Interactable GetClosest(Node2D interactor) {
        return s_Me.GetTree().GetNodesInGroup("Interactables").Where(node => node is Interactable interactable && interactable.CanInteract(interactor)).Cast<Interactable>().MinBy(interactable => interactable.GlobalPosition.DistanceSquaredTo(interactor.GlobalPosition));
    }

    public static bool IsActive(Interactable interactable) {
        return s_ActiveInteractables.Contains(interactable);
    }
}

public interface Interactable {

    public bool CanInteract(Node2D interactor);

    public void Interact(Node2D interactor);

    public virtual void ActivateInteract() { }
    public virtual void InactivateInteract() { }

    public Vector2 GlobalPosition { get; set; }
}