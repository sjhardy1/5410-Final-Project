using System;
using Godot;

public partial class PlacementController : Node2D
{
    [Export] public NodePath OccupancyMapPath;
    [Export] public Shader InvalidPlacementShader;

    private GridOccupancyMap occupancyMap;
    private GridPlaceable activePlaceable;
    private ShaderMaterial invalidPlacementMaterial;
    private bool activePlacementIsValid = true;

    public override void _Ready()
    {        
        occupancyMap = GetNode<GridOccupancyMap>(OccupancyMapPath);
        SetupMaterials();
        GetNode<SignalBus>("/root/SignalBus").CancelPlacement += CancelPlacement;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (activePlaceable == null || occupancyMap == null)
        {
            return;
        }

        if (@event is InputEventMouseMotion)
        {
            UpdateActivePreviewFromMouse();
        }

        if (@event.IsActionPressed("rotate_building"))
        {
            activePlaceable.RotationQuarterTurns += 1;
            activePlaceable.Rotation = Mathf.DegToRad(90 * activePlaceable.RotationQuarterTurns);
            UpdateActivePreviewFromMouse();
        }

        if (@event.IsActionPressed("place_confirm"))
        {
            Vector2I anchor = occupancyMap.WorldToCell(GetGlobalMousePosition());
            bool placed = occupancyMap.TryPlace(activePlaceable, anchor);
            if (placed)
            {
                activePlaceable.Material = null;
                activePlaceable = null;
                GetNode<SignalBus>("/root/SignalBus").PublishStopPlacing();
            }
            else
            {
                UpdateActivePreviewFromMouse();
            }
        }

        if (@event.IsActionPressed("place_cancel")) CancelPlacement();
    }

    private void CancelPlacement()
    {
        if (activePlaceable != null)
        {
            GetNode<SignalBus>("/root/SignalBus").PublishPlaceableAddedToStorage(activePlaceable.def);
            activePlaceable.QueueFree();
            activePlaceable = null;
            GetNode<SignalBus>("/root/SignalBus").PublishStopPlacing();
        }
    }

    public GridPlaceable BeginPlacement(PlaceableDefinition def)
    {
        if (def.Scene == null)
        {
            return null;
        }

        PlaceableDefinition runtimeDefinition = def.Duplicate() as PlaceableDefinition;
        if (runtimeDefinition == null)
        {
            return null;
        }

        if (activePlaceable != null)
        {
            GetNode<SignalBus>("/root/SignalBus").PublishPlaceableAddedToStorage(activePlaceable.def);
            activePlaceable.QueueFree();
        }

        activePlaceable = runtimeDefinition.Scene.Instantiate<GridPlaceable>();
        activePlaceable.Initialize(runtimeDefinition);
        AddChild(activePlaceable);
        UpdateActivePreviewFromMouse();
        GetNode<SignalBus>("/root/SignalBus").PublishPlacing();
        return activePlaceable;
    }

    private void SetupMaterials()
    {

        if (InvalidPlacementShader == null)
        {
            InvalidPlacementShader = GD.Load<Shader>("res://Shaders/invalid_placement_tint.gdshader");
        }
        if (InvalidPlacementShader != null)
        {
            invalidPlacementMaterial = new ShaderMaterial();
            invalidPlacementMaterial.Shader = InvalidPlacementShader;
        }
    }
    private void UpdateActivePreviewFromMouse()
    {
        if (activePlaceable == null || occupancyMap == null)
        {
            return;
        }

        Vector2I anchor = occupancyMap.WorldToCell(GetGlobalMousePosition());
        activePlaceable.def.AnchorCell = anchor;
        activePlaceable.GlobalPosition = occupancyMap.CellToWorld(anchor, activePlaceable.def is UnitDefinition);
        activePlaceable.ZIndex = anchor.Y + 10;

        activePlacementIsValid = occupancyMap.CanPlace(activePlaceable, anchor);
        activePlaceable.Material = activePlacementIsValid ? null : invalidPlacementMaterial;
    }
}
