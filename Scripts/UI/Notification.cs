using Godot;
using System;

public partial class Notification : CanvasLayer
{
    [Export] private NodePath msgPath;
    public void Initialize(string message)
    {
        GetNode<Label>(msgPath).Text = message;
        Timer timer = new Timer
        {
            WaitTime = 3f,
            Autostart = true
        };
        AddChild(timer);
        timer.Timeout += QueueFree;
    }
}
