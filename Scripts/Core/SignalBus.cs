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
    public delegate void BuildingDestroyedEventHandler(Node buildingNode);

    [Signal]
    public delegate void UnitDiedEventHandler(int combatantUid);

    [Signal]
    public delegate void RaidEndedEventHandler();

    [Signal]
    public delegate void PauseToggledEventHandler(bool isPaused);

    [Signal]
    public delegate void ErrorRaisedEventHandler(string source, string message);
    
    [Signal]
    public delegate void RaidBeginEventHandler();
    [Signal]
    public delegate void PlaceablePlacedEventHandler(GridPlaceable placeable);    
    public void PublishPlaceablePlaced(GridPlaceable placeable)
    {
        EmitSignal(nameof(PlaceablePlaced), placeable);
    }

    [Signal]
    public delegate void PlaceableAddedToStorageEventHandler(GridPlaceable placeable);

    public void PublishPlaceableAddedToStorage(GridPlaceable placeable)
    {
        EmitSignal(nameof(PlaceableAddedToStorage), placeable);
    }

    [Signal]
    public delegate void PlaceableRemovedFromStorageEventHandler(GridPlaceable placeable);

    public void PublishPlaceableRemovedFromStorage(GridPlaceable placeable)
    {
        EmitSignal(nameof(PlaceableRemovedFromStorage), placeable);
    }

    [Signal]
    public delegate void PlaceableRemovedFromActiveEventHandler(GridPlaceable placeable);

    public void PublishPlaceableRemovedFromActive(GridPlaceable placeable)
    {
        EmitSignal(nameof(PlaceableRemovedFromActive), placeable);
    }

    [Signal]
    public delegate void ClearCellsEventHandler();
    public void PublishClearCells()
    {
        EmitSignal(nameof(ClearCells));
    }

    [Signal]
    public delegate void CancelPlacementEventHandler();
    public void PublishCancelPlacement()
    {
        EmitSignal(nameof(CancelPlacement));
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

    public void PublishBuildingDestroyed(Node buildingNode)
    {
        EmitSignal(nameof(BuildingDestroyed), buildingNode);
    }

    public void PublishUnitDied(int combatantUid)
    {
        EmitSignal(nameof(UnitDied), combatantUid);
    }

    public void PublishRaidEnded()
    {
        EmitSignal(nameof(RaidEnded));
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
