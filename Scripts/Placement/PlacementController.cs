using System;
using Godot;

public partial class PlacementController : Node2D
{
    [Export] public NodePath OccupancyMapPath;
    [Export] public Shader InvalidPlacementShader;

    public GridOccupancyMap occupancyMap;
    private GridPlaceable activePlaceable;
    private ShaderMaterial invalidPlacementMaterial;
    private bool activePlacementIsValid = true;

    public GridPlaceable ActivePlaceable => activePlaceable;

    public override void _Ready()
    {        
        occupancyMap = GetNode<GridOccupancyMap>(OccupancyMapPath);
        SetupMaterials();
        GetNode<SignalBus>("/root/SignalBus").CancelPlacement += CancelPlacement;
    }
    public override void _ExitTree()
    {
        GetNode<SignalBus>("/root/SignalBus").CancelPlacement -= CancelPlacement;
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
            RemoveChild(activePlaceable);
            GetNode<SignalBus>("/root/SignalBus").PublishPlaceableAddedToStorage(activePlaceable);
            activePlaceable = null;
            GetNode<SignalBus>("/root/SignalBus").PublishStopPlacing();
        }
    }
    public void BeginPlacement(GridPlaceable placeable)
    {
        if (placeable.def.Scene == null)
        {
            return;
        }

        if (activePlaceable != null)
        {
            RemoveChild(activePlaceable);
            GetNode<SignalBus>("/root/SignalBus").PublishPlaceableAddedToStorage(activePlaceable);
        }

        activePlaceable = placeable;
        AddChild(activePlaceable);
        UpdateActivePreviewFromMouse();
        GetNode<SignalBus>("/root/SignalBus").PublishPlacing();
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
        activePlaceable.AnchorCell = anchor;
        activePlaceable.GlobalPosition = occupancyMap.CellToWorld(anchor, activePlaceable.def is UnitDefinition);
        activePlaceable.ZIndex = anchor.Y + 10;

        activePlacementIsValid = occupancyMap.CanPlace(activePlaceable, anchor);
        activePlaceable.Material = activePlacementIsValid ? null : invalidPlacementMaterial;
    }
}
