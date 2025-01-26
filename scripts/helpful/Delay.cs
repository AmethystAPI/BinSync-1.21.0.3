using System;
using Godot;

public partial class Delay : Node {
    private static Delay s_Me;

    public override void _Ready() {
        s_Me = this;
    }

    public static void Execute(float delay, Action action) {
        Timer delayNode = new Timer();

        s_Me.AddChild(delayNode);

        delayNode.WaitTime = delay;
        delayNode.Start();

        delayNode.Timeout += () => {
            s_Me.RemoveChild(delayNode);

            action.Invoke();
        };
    }
}