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
    private RaidController raidController;
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
            DeactivateChoiceScreen();
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
        foreach(GridPlaceable placeable in runState.ActivePlaceables)
        {
            if(placeable.def is BuildingDefinition buildingDef)
            {
                runState.AddResources(buildingDef.ProductionFoodPerRound, buildingDef.ProductionWoodPerRound);
                if(runState.TrySpendResources(0, buildingDef.UpkeepWoodPerRound))
                {
                    runState.ForceSpendResources(placeable.def.UpkeepFoodPerRound, 0);
                }
                else
                {
                    runState.ForceSpendResources(placeable.def.UpkeepFoodPerRound * 2 + buildingDef.UpkeepWoodPerRound, 0);
                }
            }
            if(placeable.def is UnitDefinition)
            {
                runState.ForceSpendResources(placeable.def.UpkeepFoodPerRound, 0);
            }
        }
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
