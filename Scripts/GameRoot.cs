using Godot;
using System;
using Godot.Collections;

public partial class GameRoot : Node2D
{
    private ControllableCamera camera;
    private ChoiceScreen choiceScreen;
    private CanvasLayer hud;
    private RunState runState;
    public override void _Ready()
    {
        camera = GetNode<ControllableCamera>("ControllableCamera");
        camera.Initialize(GetViewport().GetVisibleRect());

        choiceScreen = GetNode<ChoiceScreen>("ChoiceScreen");
        hud = GetNode<CanvasLayer>("HUD");

        runState = GetNode<RunState>("/root/RunState");

        Button upgradeButton = hud.GetNode<Button>("Button");
        upgradeButton.Pressed += ActivateChoiceScreen;
    }
    private void ActivateChoiceScreen()
    {
        if(!runState.TrySpendResources(20, 0)) return;
        camera.DisableControls();
        hud.Hide();
        choiceScreen.Show();
        ChoiceCard card1 = choiceScreen.AddCard(new Dictionary<string, string>{
            {"Title", "Wheat Farm"},
            {"Description", "Produces 20 food per round."}
        });
        card1.OnCardClicked += () => {
            GD.Print("Wheat Farm chosen!");
            DeactivateChoiceScreen();
        };
        ChoiceCard card2 = choiceScreen.AddCard(new Dictionary<string, string>{
            {"Title", "Sheep Farm"},
            {"Description", "Produces 10 food per round."}
        });
        card2.OnCardClicked += () => {
            GD.Print("Sheep Farm chosen!");
            DeactivateChoiceScreen();
        };
    }
    private void DeactivateChoiceScreen()
    {
        choiceScreen.ClearCards();
        choiceScreen.Hide();
        hud.Show();
        camera.EnableControls();
    }
}
