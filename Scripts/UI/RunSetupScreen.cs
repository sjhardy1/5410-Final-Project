using Godot;
using System;

public partial class RunSetupScreen : CanvasLayer
{
    [Export] private NodePath startButtonPath;
    [Export] private NodePath kitOptionButtonPath;
    [Export] private NodePath difficultyOptionButtonPath;
    public override void _Ready()
    {
        Button startButton = GetNode<Button>(startButtonPath);
        OptionButton kitOptionButton = GetNode<OptionButton>(kitOptionButtonPath);
        OptionButton difficultyOptionButton = GetNode<OptionButton>(difficultyOptionButtonPath);
        RunState runState = GetNode<RunState>("/root/RunState");

        if(runState.HasUpgrade("kit_1")) kitOptionButton.SetItemDisabled(1, false);

        startButton.Pressed += () => {
            runState.kitId = kitOptionButton.Selected;
            runState.difficulty = difficultyOptionButton.Selected;
            GetNode<GameManager>("/root/GameManager").StartNewGame();
        };
    }
}
