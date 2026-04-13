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
	public void GenerateCards(GameDatabase database, int num, LootType ?type = null, int ?round = null)
	{
		Array<LootDefinition> lootPool = database.GetAllLoot();
		var filteredLoot = new Dictionary<LootDefinition, float>();
		foreach (var loot in lootPool)
		{
			if(type != null  && !(type == loot.LootType)) continue;
			if(round != null && loot.LootAttributes.UnlockWave > round) continue;
			switch (loot.LootAttributes.Rarity)
			{
				case Rarity.Common:
					filteredLoot.Add(loot, 10f);
					break;
				case Rarity.Uncommon:
					filteredLoot.Add(loot, 4f);
					break;
				case Rarity.Rare:
					filteredLoot.Add(loot, 2f);
					break;
				case Rarity.Epic:
					filteredLoot.Add(loot, 1f);
					break;
				case Rarity.Legendary:
					filteredLoot.Add(loot, 0.5f);
					break;
			}
		}
		for (int i = 0; i < num; i++)
		{
			LootDefinition chosenLoot = WeightedRandom(filteredLoot);
			ChoiceCard card = AddCard(new Dictionary<string, string>{
				{"Title", chosenLoot.CoreAttributes.DisplayName},
				{"Rarity", chosenLoot.LootAttributes.Rarity.ToString()},
				{"Type", chosenLoot.LootType.ToString()},
				{"Description", chosenLoot.CoreAttributes.Description}
			});
			card.GetButton().Pressed += () =>
			{
				ClearCards();
				GetNode<SignalBus>("/root/SignalBus").PublishChoicePicked(new Dictionary<string, Variant>
				{
					{"Id", chosenLoot.CoreAttributes.Id},
				});
			};
		}
	}
	private LootDefinition WeightedRandom(Dictionary<LootDefinition, float> lootPool)
	{
		float totalWeight = 0f;
		foreach (var weight in lootPool.Values)
		{
			totalWeight += weight;
		}
		float randomValue = (float)GD.RandRange(0, totalWeight);
		foreach (var kvp in lootPool)
		{
			if (randomValue < kvp.Value)
			{
				return kvp.Key;
			}
			randomValue -= kvp.Value;
		}
		return null; // Should never reach here if weights are correct
	}
}
