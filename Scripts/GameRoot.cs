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
    private Hud hud;
    private RunState runState;
    private int nextUid = 1;
    public override void _Ready()
    {		
        database = ResourceLoader.Load<GameDatabase>("res://Resources/Definitions/GameDatabase.tres");

        camera = GetNode<ControllableCamera>("ControllableCamera");
        camera.Initialize(GetViewport().GetVisibleRect());

        choiceScreen = GetNode<ChoiceScreen>("ChoiceScreen");
        hud = GetNode<Hud>("HUD");
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
            LootDefinition chosenLoot = database.GetLootById(chosenId);
            if(chosenLoot is PlaceableDefinition def)
            {
                def.Uid = nextUid++;
                placementController.BeginPlacement(def);
            }
            GD.Print($"Player picked: {chosenLoot.CoreAttributes.DisplayName}");
            DeactivateChoiceScreen();
        };        
        signalBus.PlaceablePlaced += (PlaceableDefinition def) =>
        {
            runState.ActivePlaceables.Add(def);
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
        signalBus.PlaceableAddedToStorage += (PlaceableDefinition def) =>
        {
            runState.StoredPlaceables.Add(def);
            hud.UpdateStorage();
        };
        signalBus.PlaceableRemovedFromStorage += (PlaceableDefinition def) =>
        {
            runState.StoredPlaceables.Remove(def);
            placementController.BeginPlacement(def);
        };
        signalBus.PlaceableRemovedFromActive += (PlaceableDefinition def) =>
        {
            runState.ActivePlaceables.Remove(def);
            placementController.BeginPlacement(def);
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
        foreach(PlaceableDefinition def in runState.ActivePlaceables)
        {
            if(def is BuildingDefinition buildingDef)
            {
                runState.AddResources(buildingDef.ProductionFoodPerRound, buildingDef.ProductionWoodPerRound);
                if(runState.TrySpendResources(0, buildingDef.UpkeepWoodPerRound))
                {
                    runState.ForceSpendResources(def.UpkeepFoodPerRound, 0);
                }
                else
                {
                    runState.ForceSpendResources(def.UpkeepFoodPerRound * 2 + buildingDef.UpkeepWoodPerRound, 0);
                }
            }
            if(def is UnitDefinition)
            {
                runState.ForceSpendResources(def.UpkeepFoodPerRound, 0);
            }
        }
        ActivateChoiceScreen();
    }
}
