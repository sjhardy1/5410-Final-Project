using Godot;
using System;
using Godot.Collections;
using System.Collections;

public partial class GameRoot : Node2D
{
    private ControllableCamera camera;
    private ChoiceScreen choiceScreen;
    private GameDatabase database;
    private SignalBus signalBus;
    private PlacementController placementController;
    private Hud hud;
    private RunState runState;
    private Queue lootQueue = new Queue();
    private int nextUid = 1;
    private bool choiceScreenActive = false;
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

        runState.StoredPlaceables.Add(database.GetLootById("wheat_farm") as PlaceableDefinition);
        runState.StoredPlaceables.Add(database.GetLootById("warrior") as PlaceableDefinition);
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
    public override void _Process(double delta)
    {
        if (!choiceScreenActive && lootQueue.Count > 0)
        {
            PendingLoot pendingLoot = (PendingLoot)lootQueue.Dequeue();
            ActivateChoiceScreen();
            choiceScreen.GenerateCards(database, pendingLoot.num, pendingLoot.lootType, runState.Wave);
        }
    }
    private void ActivateChoiceScreen()
    {
        choiceScreenActive = true;
        camera.DisableControls();
        hud.Hide();
        choiceScreen.Show();
    }
    private void DeactivateChoiceScreen()
    {
        choiceScreenActive = false;
        choiceScreen.Hide();
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
        lootQueue.Enqueue(new PendingLoot(3));
    }
}
