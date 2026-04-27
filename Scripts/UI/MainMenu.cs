using Godot;
using System;

public partial class MainMenu : CanvasLayer
{
	[Export] public NodePath newGameButton;
	[Export] public NodePath loadGameButton;
	[Export] public NodePath upgradesButton;
	[Export] public NodePath settingsButton;
	[Export] public NodePath exitButton;
	[Export] private PackedScene notifScene;
	public override void _Ready()
	{
		GameManager gm = GetNode<GameManager>("/root/GameManager");
		GetNode<Button>(newGameButton).Pressed += gm.StartNewGame;
		GetNode<Button>(loadGameButton).Pressed += TryLoadGame;
		GetNode<Button>(upgradesButton).Pressed += () => gm.ChangeScene("upgrades");
		GetNode<Button>(settingsButton).Pressed += () => gm.ChangeScene("settings");
		GetNode<Button>(exitButton).Pressed += () => GetTree().Quit();
	}
	private void TryLoadGame(){
		GameManager gm = GetNode<GameManager>("/root/GameManager");
		if (!gm.LoadGame())
		{
			// Show notification that no save was found.
			Notification notification = notifScene.Instantiate() as Notification;
			notification.Initialize("No saved run found.");
			AddChild(notification);
		}
	}
}
