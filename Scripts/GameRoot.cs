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
	private SignalBus signalBus;
	private PlacementController placementController;
	private RaidController raidController;
	private Hud hud;
	private RunState runState;
	private Queue lootQueue = new Queue();
	private int nextUid = 1;
	private bool isScreenBusy = false;
	[Export] private PackedScene upkeepReportScene;
	public override void _Ready()
	{		
		database = ResourceLoader.Load<GameDatabase>("res://Resources/Definitions/GameDatabase.tres");

		camera = GetNode<ControllableCamera>("ControllableCamera");
		camera.Initialize(GetViewport().GetVisibleRect());

		choiceScreen = GetNode<ChoiceScreen>("ChoiceScreen");
		hud = GetNode<Hud>("HUD");
		placementController = GetNode<PlacementController>("PlacementController");
		raidController = GetNode<RaidController>("RaidController");

		runState = GetNode<RunState>("/root/RunState");
		signalBus = GetNode<SignalBus>("/root/SignalBus");

		runState.StoredPlaceables.Add(InstantiatePlaceable(database.GetLootById("wheat_farm") as PlaceableDefinition));
		runState.StoredPlaceables.Add(InstantiatePlaceable(database.GetLootById("warrior") as PlaceableDefinition));
		hud.UpdateStorage();

		Button recruitButton = hud.GetNode<Button>("Control/RecruitButton");
		recruitButton.Pressed += () =>
		{
			if(runState.TrySpendResources(50, 0))
			{
				lootQueue.Enqueue(new PendingLoot(2, LootType.Unit, runState.Wave));
			}
		};
		Button constructButton = hud.GetNode<Button>("Control/ConstructButton");
		constructButton.Pressed += () =>
		{            
			if(runState.TrySpendResources(0, 50))
			{
				runState.pendingConstruction++;
			}
		};

		signalBus.ChoicePicked += (Dictionary<string, Variant> choiceData) =>
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
		};        
		signalBus.PlaceablePlaced += (GridPlaceable placeable) =>
		{
			runState.ActivePlaceables.Add(placeable);
		};
		signalBus.RaidEnded += () =>
		{
			foreach(Node child in placementController.GetChildren())
			{
				if(child is GridPlaceable placeable)
				{
					placeable.Show();
				}
			}
			runState.AdvanceWave();
			runState.StartDowntime();
			ResolveUpkeep();
		};
		signalBus.RaidBegin += () =>
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
		};
		signalBus.PlaceableAddedToStorage += (GridPlaceable placeable) =>
		{
			runState.StoredPlaceables.Add(placeable);
			hud.UpdateStorage();
		};
		signalBus.PlaceableRemovedFromStorage += (GridPlaceable placeable) =>
		{
			runState.StoredPlaceables.Remove(placeable);
			placementController.BeginPlacement(placeable);
		};
		signalBus.PlaceableRemovedFromActive += (GridPlaceable placeable) =>
		{
			placementController.RemoveChild(placeable);
			runState.ActivePlaceables.Remove(placeable);
			placementController.BeginPlacement(placeable);
		};

		GridPlaceable townCenter = InstantiatePlaceable(database.GetLootById("town_center") as PlaceableDefinition);
		placementController.AddChild(townCenter);
		placementController.occupancyMap.TryPlace(townCenter, Vector2I.Zero);
	}
	public override void _Process(double delta)
	{
		if (!isScreenBusy && lootQueue.Count > 0)
		{
			PendingLoot pendingLoot = (PendingLoot)lootQueue.Dequeue();
			MarkScreenAsBusy();
			choiceScreen.Show();
			choiceScreen.GenerateCards(database, pendingLoot.num, pendingLoot.lootType, runState.Wave);
		}
	}
	private void MarkScreenAsBusy()
	{
		isScreenBusy = true;
		camera.DisableControls();
		hud.Hide();
	}
	private void MarkScreenAsFree()
	{
		isScreenBusy = false;
		hud.Show();
		camera.EnableControls();
	}
	private void ResolveUpkeep()
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
	private GridPlaceable InstantiatePlaceable(PlaceableDefinition def)
	{
		PlaceableDefinition runtimeDefinition = def.Duplicate() as PlaceableDefinition;
		GridPlaceable placeable = def.Scene.Instantiate<GridPlaceable>();
		placeable.Initialize(runtimeDefinition);
		return placeable;
	}
}
