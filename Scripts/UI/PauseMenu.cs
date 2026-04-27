using Godot;
using System;

public partial class PauseMenu : CanvasLayer
{
    [Export] private NodePath resumeButtonPath;
    [Export] private NodePath mainMenuButtonPath;
    public override void _Ready()
    {
        GetNode<Button>(resumeButtonPath).Pressed += QueueFree;
        GetNode<Button>(mainMenuButtonPath).Pressed += () =>
        {
            GetNode<GameManager>("/root/GameManager").ChangeScene("menu");
            QueueFree();
        };
    }
    public override void _ExitTree()
    {
        GetNode<RunState>("/root/RunState").SetPaused(false);
    }
}
