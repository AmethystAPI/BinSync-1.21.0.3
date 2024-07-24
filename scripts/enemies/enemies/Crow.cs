using System.Collections.Generic;

public partial class Crow : Enemy
{
    public static List<Crow> Crows = new List<Crow>();

    public override void _EnterTree()
    {
        Crows.Add(this);
    }

    public override void _ExitTree()
    {
        Crows.Remove(this);
    }
}
