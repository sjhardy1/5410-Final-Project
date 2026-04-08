using Godot;
using System;
using Godot.Collections;

public partial class SignalBus : Node
{
    [Signal]
    public delegate void ChoiceScreenRequestedEventHandler(Array<Dictionary<string, Variant>> options);

    [Signal]
    public delegate void ChoicePickedEventHandler(Dictionary<string, Variant> choiceData);

    [Signal]
    public delegate void BuildingPlacedEventHandler(Node buildingNode, Vector2 worldPosition);

    [Signal]
    public delegate void BuildingDestroyedEventHandler(Node buildingNode);

    [Signal]
    public delegate void UnitSpawnedEventHandler(Node unitNode, bool isEnemy);

    [Signal]
    public delegate void UnitDiedEventHandler(Node unitNode, bool isEnemy);

    [Signal]
    public delegate void RaidStartedEventHandler(int wave);

    [Signal]
    public delegate void RaidEndedEventHandler(int wave, bool victory);

    [Signal]
    public delegate void PauseToggledEventHandler(bool isPaused);

    [Signal]
    public delegate void ErrorRaisedEventHandler(string source, string message);

    public void PublishChoiceScreenRequested(Array<Dictionary<string, Variant>> options)
    {
        EmitSignal(nameof(ChoiceScreenRequested), options);
    }

    public void PublishChoicePicked(Dictionary<string, Variant> choiceData)
    {
        EmitSignal(nameof(ChoicePicked), choiceData);
    }

    public void PublishBuildingPlaced(Node buildingNode, Vector2 worldPosition)
    {
        EmitSignal(nameof(BuildingPlaced), buildingNode, worldPosition);
    }

    public void PublishBuildingDestroyed(Node buildingNode)
    {
        EmitSignal(nameof(BuildingDestroyed), buildingNode);
    }

    public void PublishUnitSpawned(Node unitNode, bool isEnemy)
    {
        EmitSignal(nameof(UnitSpawned), unitNode, isEnemy);
    }

    public void PublishUnitDied(Node unitNode, bool isEnemy)
    {
        EmitSignal(nameof(UnitDied), unitNode, isEnemy);
    }

    public void PublishRaidStarted(int wave)
    {
        EmitSignal(nameof(RaidStarted), wave);
    }

    public void PublishRaidEnded(int wave, bool victory)
    {
        EmitSignal(nameof(RaidEnded), wave, victory);
    }

    public void PublishPauseToggled(bool isPaused)
    {
        EmitSignal(nameof(PauseToggled), isPaused);
    }

    public void PublishError(string source, string message)
    {
        EmitSignal(nameof(ErrorRaised), source, message);
    }
}
