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
	[Export] private PackedScene confirmationScene;
	public override void _Ready()
	{
		GameManager gm = GetNode<GameManager>("/root/GameManager");
		RunState runState = GetNode<RunState>("/root/RunState");
		SaveManager saveManager = GetNode<SaveManager>("/root/SaveManager");

		// Load meta state at startup
		Godot.Collections.Dictionary<string, Variant> metaData = saveManager.LoadMetaState();
		runState.LoadMetaState(metaData);

		GetNode<Button>(newGameButton).Pressed += TryStartNewGame;
		GetNode<Button>(loadGameButton).Pressed += TryLoadGame;
		GetNode<Button>(upgradesButton).Pressed += () => gm.ChangeScene("upgrades");
		GetNode<Button>(upgradesButton).Text = "Purchase Upgrades (" + runState.MetaCurrency + " Crystals)";
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
			RunState runState = GetNode<RunState>("/root/RunState");
			runState.sfxPlayer.Stream = runState.audioCache["denied"];
			runState.sfxPlayer.Play();
		}
	}
	private void TryStartNewGame()
	{
		GameManager gm = GetNode<GameManager>("/root/GameManager");
		if(!GetNode<SaveManager>("/root/SaveManager").HasRunSave()) {
			gm.SetupNewGame();
			return;
		}
		// Show confirmation dialog before starting a new game.
		Confirmation confirmation = confirmationScene.Instantiate() as Confirmation;
		confirmation.Initialize("This action will erase previous save data. Continue?", gm.SetupNewGame);
		AddChild(confirmation);
	}
}
