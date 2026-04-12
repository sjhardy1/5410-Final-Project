using Godot;
using System;
using Godot.Collections;

public partial class GameRoot : Node2D
{
    private ControllableCamera camera;
    private ChoiceScreen choiceScreen;
    private GameDatabase database;
    private SignalBus signalBus;
    private PlacementController placementController;
    private CanvasLayer hud;
    private RunState runState;
    public override void _Ready()
    {		
        database = ResourceLoader.Load<GameDatabase>("res://Resources/Definitions/GameDatabase.tres");

        camera = GetNode<ControllableCamera>("ControllableCamera");
        camera.Initialize(GetViewport().GetVisibleRect());

        choiceScreen = GetNode<ChoiceScreen>("ChoiceScreen");
        hud = GetNode<CanvasLayer>("HUD");
        placementController = GetNode<PlacementController>("PlacementController");

        runState = GetNode<RunState>("/root/RunState");
        signalBus = GetNode<SignalBus>("/root/SignalBus");

        Button upgradeButton = hud.GetNode<Button>("Button");
        upgradeButton.Pressed += () =>
        {
            if(runState.TrySpendResources(60, 0))
            {
                ActivateChoiceScreen();
            }
        };

        signalBus.ChoicePicked += (Dictionary<string, Variant> choiceData) =>
        {
            string chosenId = (string)choiceData["Id"];
            LootDefinitionModel chosenLoot = database.GetLootById(chosenId);
            placementController.BeginPlacement(chosenLoot.Scene, chosenLoot.CoreAttributes.Id, chosenLoot.Footprint, chosenLoot.LootType);
            GD.Print($"Player picked: {chosenLoot.CoreAttributes.DisplayName}");
            DeactivateChoiceScreen();
        };        
        signalBus.BuildingPlaced += (string id, Vector2 worldPosition) =>
        {
            BuildingDefinition def = database.GetBuilding(id);
            if (def != null)
            {
                runState.ActiveBuildings.Add(def);
            }
        };
        signalBus.UnitPlaced += (string id, Vector2 worldPosition) =>
        {
            UnitDefinition def = database.GetUnit(id);
            if (def != null)
            {
                runState.ActiveUnits.Add(def);
            }
        };
        signalBus.RaidEnded += (int wave) =>
        {
            runState.AdvanceWave();
            runState.StartDowntime();
            ResolveUpkeep();
        };
        signalBus.RaidBegin += () =>
        {
            runState.StartRaid();
        };
        signalBus.PlaceableAddedToStorage += (string id, string lootType) =>
        {
            if(lootType == LootType.Building.ToString())
            {
                BuildingDefinition def = database.GetBuilding(id);
                if (def != null)
                {
                    runState.StoredBuildings.Add(def);
                }
            }
            else if(lootType == LootType.Unit.ToString())
            {
                UnitDefinition def = database.GetUnit(id);
                if (def != null)
                {
                    runState.StoredUnits.Add(def);
                }
            }
        };
    }
    private void ActivateChoiceScreen()
    {
        camera.DisableControls();
        hud.Hide();
        choiceScreen.Show();
        choiceScreen.GenerateCards(database, 3);
    }
    private void DeactivateChoiceScreen()
    {
        choiceScreen.Hide();
        hud.Show();
        camera.EnableControls();
    }
    private void ResolveUpkeep()
    {
        foreach(BuildingDefinition def in runState.ActiveBuildings)
        {
            runState.AddResources(def.ProductionFoodPerRound, def.ProductionWoodPerRound);
            if(runState.TrySpendResources(0, def.UpkeepWoodPerRound))
            {
                runState.ForceSpendResources(def.UpkeepFoodPerRound, 0);
            }
            else
            {
                runState.ForceSpendResources(def.UpkeepFoodPerRound * 2 + def.UpkeepWoodPerRound, 0);
            }
        }
        foreach(UnitDefinition def in runState.ActiveUnits)
        {
            runState.ForceSpendResources(def.UpkeepFoodPerRound, 0);
        }
        ActivateChoiceScreen();
    }
}
