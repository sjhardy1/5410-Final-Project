using Godot;
using System;

public partial class Confirmation : CanvasLayer
{
    [Export] private NodePath msgPath;
    [Export] private NodePath confirmButtonPath;
    [Export] private NodePath cancelButtonPath;
    public void Initialize(string message, Action onConfirm)
    {
        GetNode<Label>(msgPath).Text = message;
        GetNode<Button>(confirmButtonPath).Pressed += () =>
        {
            onConfirm();
            QueueFree();
        };
        GetNode<Button>(cancelButtonPath).Pressed += QueueFree;
    }
}
