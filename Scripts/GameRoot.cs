using Godot;
using System;
using Godot.Collections;

public partial class GameRoot : Node2D
{
    private ControllableCamera camera;
    private ChoiceScreen choiceScreen;
    private GameDatabase database;
    private SignalBus signalBus;
    private CanvasLayer hud;
    private RunState runState;
    public override void _Ready()
    {		
        database = ResourceLoader.Load<GameDatabase>("res://Resources/Definitions/GameDatabase.tres");

        camera = GetNode<ControllableCamera>("ControllableCamera");
        camera.Initialize(GetViewport().GetVisibleRect());

        choiceScreen = GetNode<ChoiceScreen>("ChoiceScreen");
        hud = GetNode<CanvasLayer>("HUD");

        runState = GetNode<RunState>("/root/RunState");
        signalBus = GetNode<SignalBus>("/root/SignalBus");

        Button upgradeButton = hud.GetNode<Button>("Button");
        upgradeButton.Pressed += ActivateChoiceScreen;

        signalBus.ChoicePicked += (Dictionary<string, Variant> choiceData) =>
        {
            string chosenId = (string)choiceData["Id"];
            LootDefinitionModel chosenLoot = database.GetLootById(chosenId);
            GD.Print($"Player picked: {chosenLoot.CoreAttributes.DisplayName}");
            DeactivateChoiceScreen();
        };
    }
    private void ActivateChoiceScreen()
    {
        if(!runState.TrySpendResources(20, 0)) return;
        camera.DisableControls();
        hud.Hide();
        choiceScreen.Show();
        choiceScreen.GenerateCards(database, 3, LootType.Building);
    }
    private void DeactivateChoiceScreen()
    {
        choiceScreen.Hide();
        hud.Show();
        camera.EnableControls();
    }
}
