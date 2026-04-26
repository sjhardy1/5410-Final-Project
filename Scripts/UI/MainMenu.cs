using Godot;
using System;

public partial class MainMenu : CanvasLayer
{
	[Export] public NodePath newGameButton;
	[Export] public NodePath loadGameButton;
	[Export] public NodePath upgradesButton;
	[Export] public NodePath settingsButton;
	[Export] public NodePath exitButton;
	public override void _Ready()
	{
		GameManager gm = GetNode<GameManager>("/root/GameManager");
		GetNode<Button>(newGameButton).Pressed += () => gm.ChangeScene("game_root");
		GetNode<Button>(upgradesButton).Pressed += () => gm.ChangeScene("upgrades");
		GetNode<Button>(settingsButton).Pressed += () => gm.ChangeScene("settings");
		GetNode<Button>(exitButton).Pressed += () => GetTree().Quit();
	}
}
