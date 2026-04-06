using Godot;
using System;
using Godot.Collections;

public partial class ChoiceScreen : CanvasLayer
{
	[Export]
    public PackedScene choiceCardScene;
	private Control cardContainer;
	public override void _Ready()
	{
		cardContainer = GetNode<Control>("Control/ScrollContainer/HBoxContainer");
		ClearCards();
	}
	public void ClearCards()
	{
		foreach (var card in cardContainer.GetChildren())
		{
			if (card is Control control)
			{
				control.Visible = false;
			}
		}
	}
	public ChoiceCard AddCard(Dictionary<string, string> cardInfo = null)
	{
		ChoiceCard card = (ChoiceCard)choiceCardScene.Instantiate();
		card.writeCardInfo(cardInfo ?? new Dictionary<string, string>{});
		cardContainer.AddChild(card);	
		return card;
	}
}
