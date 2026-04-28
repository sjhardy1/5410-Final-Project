using Godot;
using System;
using Godot.Collections;
public partial class Upgrades : CanvasLayer
{
    [Export] Dictionary<string, NodePath> upgradeButtonPaths;
    [Export] NodePath returnButtonPath;
    RunState runState;
    public override void _Ready()
    {
        GetNode<Button>(returnButtonPath).Pressed += () => GetNode<GameManager>("/root/GameManager").ChangeScene("menu");
        runState = GetNode<RunState>("/root/RunState");
    }
}
