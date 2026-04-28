using Godot;
using System;
using Godot.Collections;
using System.Collections;
using Collections = System.Collections.Generic;

public partial class GameRoot : Node2D
{
	private ControllableCamera camera;
	private ChoiceScreen choiceScreen;
	private GameDatabase database;
	private SaveManager saveManager;
	private SignalBus signalBus;
	private PlacementController placementController;
	private RaidController raidController;
	private Hud hud;
	private Button pauseButton;
	private Button recruitButton;
	private Button constructButton;
	private RunState runState;
	private Queue lootQueue = new Queue();
	private int nextUid = 1;
	private bool isScreenBusy = false;
	private int maxWave = 12;
	[Export] private PackedScene upkeepReportScene;
	[Export] private PackedScene gameEndScene;
	[Export] private PackedScene notifScene;
	[Export] private PackedScene pauseMenuScene;
	[Export] private NodePath pauseButtonPath;
	public override void _Ready()
	{		
		database = ResourceLoader.Load<GameDatabase>("res://Resources/Definitions/GameDatabase.tres");
		saveManager = GetNode<SaveManager>("/root/SaveManager");

		camera = GetNode<ControllableCamera>("ControllableCamera");
		camera.Initialize(GetViewport().GetVisibleRect());

		choiceScreen = GetNode<ChoiceScreen>("ChoiceScreen");
		hud = GetNode<Hud>("HUD");
		placementController = GetNode<PlacementController>("PlacementController");
		raidController = GetNode<RaidController>("RaidController");

		runState = GetNode<RunState>("/root/RunState");
		signalBus = GetNode<SignalBus>("/root/SignalBus");

		recruitButton = hud.GetNode<Button>("Control/RecruitButton");
		recruitButton.Pressed += OnRecruitButtonPressed;
		constructButton = hud.GetNode<Button>("Control/ConstructButton");
		constructButton.Pressed += OnConstructButtonPressed;
		pauseButton = GetNode<Button>(pauseButtonPath);
		pauseButton.Pressed += PauseGame;

		signalBus.ChoicePicked += OnChoicePicked;        
		signalBus.PlaceablePlaced += OnPlaceablePlaced;
		signalBus.RaidEnded += OnRaidEnded;
		signalBus.RaidBegin += OnRaidBegin;
		signalBus.PlaceableAddedToStorage += OnPlaceableAddedToStorage;
		signalBus.PlaceableRemovedFromStorage += OnPlaceableRemovedFromStorage;
		signalBus.PlaceableRemovedFromActive += OnPlaceableRemovedFromActive;
		signalBus.GameLost += OnGameLost;
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		if (gameManager.ConsumeLoadSavedRunOnGameRoot() && TryLoadSavedRun())
		{

		} else {
			InitializeNewRun();
		}
		maxWave = runState.difficulty * 2 + 6;
		hud.Initialize(maxWave);
	}
	public override void _ExitTree()
	{
		if (recruitButton != null)
		{
			recruitButton.Pressed -= OnRecruitButtonPressed;
		}
		if (constructButton != null)
		{
			constructButton.Pressed -= OnConstructButtonPressed;
		}
		if (signalBus != null)
		{
			signalBus.ChoicePicked -= OnChoicePicked;
			signalBus.PlaceablePlaced -= OnPlaceablePlaced;
			signalBus.RaidEnded -= OnRaidEnded;
			signalBus.RaidBegin -= OnRaidBegin;
			signalBus.PlaceableAddedToStorage -= OnPlaceableAddedToStorage;
			signalBus.PlaceableRemovedFromStorage -= OnPlaceableRemovedFromStorage;
			signalBus.PlaceableRemovedFromActive -= OnPlaceableRemovedFromActive;
			signalBus.GameLost -= OnGameLost;
		}
	}
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("pause_game"))
		{
			PauseGame();
		}
	}
	public override void _Process(double delta)
	{
		if (!isScreenBusy && lootQueue.Count > 0)
		{
			PendingLoot pendingLoot = (PendingLoot)lootQueue.Dequeue();
			MarkScreenAsBusy();
			choiceScreen.Show();
			choiceScreen.GenerateCards(database, pendingLoot.num, pendingLoot.lootType);
		}
	}
	private void MarkScreenAsBusy()
	{
		isScreenBusy = true;
		camera.DisableControls();
		hud.Hide();
		pauseButton.Hide();
	}
	private void MarkScreenAsFree()
	{
		isScreenBusy = false;
		hud.Show();
		pauseButton.Show();
		camera.EnableControls();
		if (!saveManager.SaveRunState(runState, placementController.ActivePlaceable))
		{
			GD.PrintErr("Failed to save run state");
		} else {
			GD.Print("Run state saved successfully");
		}
	}
	private void ResolveUpkeep(int healCost, int repairCost)
	{

		for(int i = 0; i < runState.pendingConstruction; i++)
		{
			lootQueue.Enqueue(new PendingLoot(3, LootType.Building));
		}
		runState.pendingConstruction = 0;
		Collections.Queue<int> foodChanges = new Collections.Queue<int>();
		Collections.Queue<string> foodChangeSources = new Collections.Queue<string>();
		Collections.Queue<int> woodChanges = new Collections.Queue<int>();
		Collections.Queue<string> woodChangeSources = new Collections.Queue<string>();
		Dictionary<string, int[]> foodProduction = new Dictionary<string, int[]>();
		Dictionary<string, int[]> woodProduction = new Dictionary<string, int[]>();
		int totalBuildingFoodUpkeep = 0;
		int totalBuildingWoodUpkeep = 0;
		int totalUnitFoodUpkeep = 0;
		bool decreasedFoodEfficiency = false;
		runState.ForceSpendResources(healCost, repairCost);
		if(healCost > 0)
		{
			foodChangeSources.Enqueue("Unit Heal Costs");
			foodChanges.Enqueue(-healCost);
		}
		if(repairCost > 0)
		{
			woodChangeSources.Enqueue("Building Repair Costs");
			woodChanges.Enqueue(-repairCost);
		}
		foreach(GridPlaceable placeable in runState.ActivePlaceables)
		{
			if(placeable.def is BuildingDefinition buildingDef)
			{
				//Add Resources
				runState.AddResources(buildingDef.ProductionFoodPerRound, buildingDef.ProductionWoodPerRound);
				if(buildingDef.ProductionFoodPerRound != 0)
				{
					if(foodProduction.ContainsKey(buildingDef.CoreAttributes.DisplayName))
					{
						foodProduction[buildingDef.CoreAttributes.DisplayName] = 
						[
							foodProduction[buildingDef.CoreAttributes.DisplayName][0] + buildingDef.ProductionFoodPerRound, 
							foodProduction[buildingDef.CoreAttributes.DisplayName][1] + 1
						];
					} else
					{
						foodProduction[buildingDef.CoreAttributes.DisplayName] = [buildingDef.ProductionFoodPerRound, 1];
					}
				}
				if(buildingDef.ProductionWoodPerRound != 0)
				{
					if(woodProduction.ContainsKey(buildingDef.CoreAttributes.DisplayName))
					{
					   woodProduction[buildingDef.CoreAttributes.DisplayName] = 
					   [
							woodProduction[buildingDef.CoreAttributes.DisplayName][0] + buildingDef.ProductionWoodPerRound, 
							woodProduction[buildingDef.CoreAttributes.DisplayName][1] + 1
					   ];
					} else
					{
						woodProduction[buildingDef.CoreAttributes.DisplayName] = [buildingDef.ProductionWoodPerRound, 1];
					} 
				}

				//Spend Resources
				if(runState.TrySpendResources(0, buildingDef.UpkeepWoodPerRound))
				{
					runState.ForceSpendResources(placeable.def.UpkeepFoodPerRound, 0);
					totalBuildingFoodUpkeep += placeable.def.UpkeepFoodPerRound;
					totalBuildingWoodUpkeep += buildingDef.UpkeepWoodPerRound;
				}
				else
				{
					decreasedFoodEfficiency = true;
					runState.ForceSpendResources(placeable.def.UpkeepFoodPerRound * 2 + buildingDef.UpkeepWoodPerRound, 0);
					totalBuildingFoodUpkeep += placeable.def.UpkeepFoodPerRound * 2 + buildingDef.UpkeepWoodPerRound;
				}
			}
			if(placeable.def is UnitDefinition)
			{
				runState.ForceSpendResources(placeable.def.UpkeepFoodPerRound, 0);
				totalUnitFoodUpkeep += placeable.def.UpkeepFoodPerRound;
			}
		}
		foreach(string entry in foodProduction.Keys)
		{
			if(foodProduction[entry][1] > 1)
			{
				foodChangeSources.Enqueue($"{foodProduction[entry][1]}× {entry}");
			} else {
				foodChangeSources.Enqueue(entry);
			}
			foodChanges.Enqueue(foodProduction[entry][0]);
		}
		foodChangeSources.Enqueue("Building Upkeep");
		foodChanges.Enqueue(-totalBuildingFoodUpkeep);
		foodChangeSources.Enqueue("Unit Upkeep");
		foodChanges.Enqueue(-totalUnitFoodUpkeep);
		foreach(string entry in woodProduction.Keys)
		{
			if(woodProduction[entry][1] > 1)
			{
				woodChangeSources.Enqueue($"{woodProduction[entry][1]}x {entry}");
			} else {
				woodChangeSources.Enqueue(entry);
			}
			woodChanges.Enqueue(woodProduction[entry][0]);
		}
		woodChangeSources.Enqueue("Building Upkeep");
		woodChanges.Enqueue(-totalBuildingWoodUpkeep);
		UpkeepReport upkeepReport = upkeepReportScene.Instantiate<UpkeepReport>();
		upkeepReport.Initialize(foodChangeSources, foodChanges, woodChangeSources, woodChanges, runState.Food, runState.Wood, decreasedFoodEfficiency);
		AddChild(upkeepReport);
		MarkScreenAsBusy();
		upkeepReport.TreeExited += MarkScreenAsFree;
		lootQueue.Enqueue(new PendingLoot(3));
	}

	private bool TryLoadSavedRun()
	{
		if (!saveManager.HasRunSave())
		{
			return false;
		}

		if (!saveManager.LoadRunState(runState))
		{
			return false;
		}

		RestoreSavedPlaceables();
		hud.UpdateStorage();
		return true;
	}

	private void InitializeNewRun()
	{
		runState.ResetRun();
		switch(runState.kitId)
		{
			case 0:
				runState.StoredPlaceables.Add(InstantiatePlaceable(database.GetLootById("wheat_farm") as PlaceableDefinition));
				runState.StoredPlaceables.Add(InstantiatePlaceable(database.GetLootById("warrior") as PlaceableDefinition));
				break;
			case 1:
				runState.StoredPlaceables.Add(InstantiatePlaceable(database.GetLootById("sheep_farm") as PlaceableDefinition));
				runState.StoredPlaceables.Add(InstantiatePlaceable(database.GetLootById("archer") as PlaceableDefinition));
				break;
			case 2:
				runState.StoredPlaceables.Add(InstantiatePlaceable(database.GetLootById("lumber_mill") as PlaceableDefinition));
				runState.StoredPlaceables.Add(InstantiatePlaceable(database.GetLootById("sharpshooter") as PlaceableDefinition));
				break;
			case 3:
				runState.StoredPlaceables.Add(InstantiatePlaceable(database.GetLootById("workshop") as PlaceableDefinition));
				runState.StoredPlaceables.Add(InstantiatePlaceable(database.GetLootById("bulwark") as PlaceableDefinition));
				break;
			default:
				break;
		}
		hud.UpdateStorage();

		GridPlaceable townCenter = InstantiatePlaceable(database.GetLootById("town_center") as PlaceableDefinition);
		placementController.AddChild(townCenter);
		placementController.occupancyMap.TryPlace(townCenter, Vector2I.Zero);
	}

	private void FinalizeGame(bool isWin)
	{
		saveManager.DeleteRunSave();
		GameEndScreen gameEnd = gameEndScene.Instantiate<GameEndScreen>();
		int finalScore = runState.Wave + (isWin ? 10 : -1);
		gameEnd.Initialize(isWin, [runState.Wave, maxWave], finalScore);
		runState.AddMetaCurrency(finalScore);
		saveManager.SaveMetaState(runState.ToMetaData());
		AddChild(gameEnd);
		MarkScreenAsBusy();
	}

	private void RestoreSavedPlaceables()
	{
		foreach (Variant value in runState.LoadedStoredPlaceablesData)
		{
			if (value.VariantType != Variant.Type.Dictionary)
			{
				continue;
			}

			Dictionary<string, Variant> placeableData = (Dictionary<string, Variant>)value;
			GridPlaceable placeable = InstantiatePlaceableFromSave(placeableData);
			if (placeable != null)
			{
				runState.StoredPlaceables.Add(placeable);
			}
		}

		foreach (Variant value in runState.LoadedActivePlaceablesData)
		{
			if (value.VariantType != Variant.Type.Dictionary)
			{
				continue;
			}

			Dictionary<string, Variant> placeableData = (Dictionary<string, Variant>)value;
			GridPlaceable placeable = InstantiatePlaceableFromSave(placeableData);
			if (placeable == null)
			{
				continue;
			}

			Vector2I anchorCell = ReadAnchorCell(placeableData);
			placementController.AddChild(placeable);
			if (!placementController.occupancyMap.TryPlace(placeable, anchorCell))
			{
				GD.PrintErr($"Failed to restore placeable {placeable.def?.CoreAttributes?.Id} at {anchorCell}.");
			}
		}
	}

	private GridPlaceable InstantiatePlaceableFromSave(Dictionary<string, Variant> placeableData)
	{
		if (!placeableData.ContainsKey("id"))
		{
			return null;
		}

		string placeableId = (string)placeableData["id"];
		PlaceableDefinition def = database.GetLootById(placeableId) as PlaceableDefinition;
		if (def == null)
		{
			GD.PrintErr($"Could not find placeable definition for id '{placeableId}'.");
			return null;
		}

		return InstantiatePlaceable(def);
	}

	private static Vector2I ReadAnchorCell(Dictionary<string, Variant> placeableData)
	{
		if (!placeableData.ContainsKey("anchor"))
		{
			return Vector2I.Zero;
		}

		Variant anchorVariant = placeableData["anchor"];
		if (anchorVariant.VariantType != Variant.Type.Dictionary)
		{
			return Vector2I.Zero;
		}

		Dictionary<string, Variant> anchorData = (Dictionary<string, Variant>)anchorVariant;
		int x = anchorData.ContainsKey("x") ? (int)anchorData["x"] : 0;
		int y = anchorData.ContainsKey("y") ? (int)anchorData["y"] : 0;
		return new Vector2I(x, y);
	}

	private GridPlaceable InstantiatePlaceable(PlaceableDefinition def)
	{
		PlaceableDefinition runtimeDefinition = def.Duplicate() as PlaceableDefinition;
		GridPlaceable placeable = def.Scene.Instantiate<GridPlaceable>();
		placeable.Initialize(runtimeDefinition);
		return placeable;
	}
	private void OnRecruitButtonPressed()
	{
		if(runState.TrySpendResources(50, 0))
		{
			lootQueue.Enqueue(new PendingLoot(2, LootType.Unit, runState.Wave));
		} else {
			Notification notification = notifScene.Instantiate<Notification>();
			notification.Initialize("Not enough food to recruit unit!");
			AddChild(notification);
		}
	}
	private void OnConstructButtonPressed()
	{
		if(runState.TrySpendResources(0, 50))
		{
			runState.pendingConstruction++;
			Notification notification = notifScene.Instantiate<Notification>();
			notification.Initialize("Construction queued! It will appear as a loot choice at the end of the round.");
			AddChild(notification);
		} else {
			Notification notification = notifScene.Instantiate<Notification>();
			notification.Initialize("Not enough wood to construct building!");
			AddChild(notification);
		}
	}
	private void OnChoicePicked(Dictionary<string, Variant> choiceData)
	{
		string chosenId = (string)choiceData["Id"];
		LootDefinition chosenLoot = database.GetLootById(chosenId);
		chosenLoot.Uid = nextUid++;
		if(chosenLoot is PlaceableDefinition def)
		{
			placementController.BeginPlacement(InstantiatePlaceable(def));
		}
		GD.Print($"Player picked: {chosenLoot.CoreAttributes.DisplayName}");
		MarkScreenAsFree();
		choiceScreen.Hide();
	}
	private void OnPlaceablePlaced(GridPlaceable placeable)
	{
		runState.ActivePlaceables.Add(placeable);
	}
	private void OnRaidEnded(int healCost, int repairCost)
	{
		if(runState.Wave >= maxWave)
		{
			FinalizeGame(true);
			return;
		}
		foreach(Node child in placementController.GetChildren())
		{
			if(child is GridPlaceable placeable)
			{
				placeable.Show();
			}
		}
		runState.AdvanceWave();
		runState.StartDowntime();
		ResolveUpkeep(healCost, repairCost);
	}
	private void OnRaidBegin()
	{
		foreach(GridPlaceable placeable in runState.ActivePlaceables)
		{
			if(placeable.def is UnitDefinition unitDef)
			{
				GD.Print("Placing unit: " + unitDef.CoreAttributes.DisplayName+" at "+ placeable.AnchorCell);
				raidController.PlaceUnit(placeable);
			}
			else if(placeable.def is BuildingDefinition buildingDef)
			{
				GD.Print("Placing building: " + buildingDef.CoreAttributes.DisplayName+" at "+ placeable.AnchorCell);
				raidController.PlaceBuilding(placeable);
			}
		}
		foreach(Node child in placementController.GetChildren())
		{
			if(child is GridPlaceable placeable)
			{
				placeable.Hide();
			}
		}
		raidController.StartRaid(database);
		runState.StartRaid();
	}
	private void OnPlaceableAddedToStorage(GridPlaceable placeable)
	{
		runState.StoredPlaceables.Add(placeable);
		hud.UpdateStorage();
	}
	private void OnPlaceableRemovedFromStorage(GridPlaceable placeable)
	{
		runState.StoredPlaceables.Remove(placeable);
		placementController.BeginPlacement(placeable);
	}
	private void OnPlaceableRemovedFromActive(GridPlaceable placeable)
	{
		placementController.RemoveChild(placeable);
		runState.ActivePlaceables.Remove(placeable);
		placementController.BeginPlacement(placeable);
	}
	private void OnGameLost()
	{
		FinalizeGame(false);
	}
	private void PauseGame()
	{
		PauseMenu pauseMenu = pauseMenuScene.Instantiate<PauseMenu>();
		pauseMenu.TreeExited += MarkScreenAsFree;
		MarkScreenAsBusy();
		AddChild(pauseMenu);
		GetNode<RunState>("/root/RunState").SetPaused(true);
	}
}
