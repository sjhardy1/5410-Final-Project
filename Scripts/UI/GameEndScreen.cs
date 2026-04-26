using Godot;
using System;

public partial class GameEndScreen : CanvasLayer
{
	[Export] public NodePath largeMessage;
	[Export] public NodePath roundsMessage;
	[Export] public NodePath completionMessage;
	[Export] public NodePath crystalMessage;
	[Export] public NodePath button;
	public void Initialize(bool win, int[] roundsCompleted, int crystals){
		if (win)
		{
			GetNode<Label>(largeMessage).Text = "You win!";
			GetNode<Label>(completionMessage).Text = "Game completed";
		} else
		{
			GetNode<Label>(largeMessage).Text = "You lose!";
			GetNode<Label>(completionMessage).Text = "Game not completed";
		}
		GetNode<Label>(roundsMessage).Text = $"Rounds completed: {roundsCompleted[0]}/{roundsCompleted[1]}";
		GetNode<Label>(crystalMessage).Text = $"+{crystals} Crystals earned";
		GetNode<Button>(button).Pressed += () => GetNode<GameManager>("/root/GameManager").ChangeScene("menu");
	}
}
