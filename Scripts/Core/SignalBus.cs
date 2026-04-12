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
    public delegate void PlacingEventHandler();
    [Signal]
    public delegate void StopPlacingEventHandler();

    [Signal]
    public delegate void BuildingPlacedEventHandler(string id, Vector2 worldPosition);

    [Signal]
    public delegate void BuildingDestroyedEventHandler(Node buildingNode);

    [Signal]
    public delegate void UnitPlacedEventHandler(string id, Vector2 worldPosition);

    [Signal]
    public delegate void UnitDiedEventHandler(Node unitNode, bool isEnemy);

    [Signal]
    public delegate void RaidStartedEventHandler(int wave);

    [Signal]
    public delegate void RaidEndedEventHandler(int wave);

    [Signal]
    public delegate void PauseToggledEventHandler(bool isPaused);

    [Signal]
    public delegate void ErrorRaisedEventHandler(string source, string message);
    
    [Signal]
    public delegate void RaidBeginEventHandler();
    [Signal]
    public delegate void PlaceableAddedToStorageEventHandler(string id, string lootType);

    public void PublishPlaceableAddedToStorage(string id, string lootType)
    {
        EmitSignal(nameof(PlaceableAddedToStorage), id, lootType);
    }

    [Signal]
    public delegate void PlaceableRemovedFromStorageEventHandler(string id, string lootType);

    public void PublishPlaceableRemovedFromStorage(string id, string lootType)
    {
        EmitSignal(nameof(PlaceableRemovedFromStorage), id, lootType);
    }

    [Signal]
    public delegate void PlaceableRemovedFromActiveEventHandler(string id, string lootType);

    public void PublishPlaceableRemovedFromActive(string id, string lootType)
    {
        EmitSignal(nameof(PlaceableRemovedFromActive), id, lootType);
    }

    [Signal]
    public delegate void ClearCellsEventHandler(GridPlaceable gridPlaceable, Vector2I anchorCell);
    public void PublishClearCells(GridPlaceable gridPlaceable, Vector2I anchorCell)
    {
        EmitSignal(nameof(ClearCells), gridPlaceable, anchorCell);
    }

    public void PublishChoiceScreenRequested(Array<Dictionary<string, Variant>> options)
    {
        EmitSignal(nameof(ChoiceScreenRequested), options);
    }

    public void PublishChoicePicked(Dictionary<string, Variant> choiceData)
    {
        EmitSignal(nameof(ChoicePicked), choiceData);
    }

    public void PublishPlacing()
    {
        EmitSignal(nameof(Placing));
    }
    public void PublishStopPlacing()
    {
        EmitSignal(nameof(StopPlacing));
    }
    public void PublishRaidBegin()
    {
        EmitSignal(nameof(RaidBegin));
    }

    public void PublishBuildingPlaced(string id, Vector2 worldPosition)
    {
        EmitSignal(nameof(BuildingPlaced), id, worldPosition);
    }

    public void PublishBuildingDestroyed(Node buildingNode)
    {
        EmitSignal(nameof(BuildingDestroyed), buildingNode);
    }

    public void PublishUnitPlaced(string id, Vector2 worldPosition)
    {
        EmitSignal(nameof(UnitPlaced), id, worldPosition);
    }

    public void PublishUnitDied(Node unitNode, bool isEnemy)
    {
        EmitSignal(nameof(UnitDied), unitNode, isEnemy);
    }

    public void PublishRaidStarted(int wave)
    {
        EmitSignal(nameof(RaidStarted), wave);
    }

    public void PublishRaidEnded(int wave)
    {
        EmitSignal(nameof(RaidEnded), wave);
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
