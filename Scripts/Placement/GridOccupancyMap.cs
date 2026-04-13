using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class GridOccupancyMap : Node
{
    [Export] public Vector2I CellSize { get; set; } = new Vector2I(64, 64);
    [Export] public Vector2I BoundsMinCell { get; set; } = new Vector2I(-10, -10);
    [Export] public Vector2I BoundsMaxCell { get; set; } = new Vector2I(10, 10);

    private readonly System.Collections.Generic.Dictionary<Vector2I, PlaceableDefinition> occupiedCells = new();

    public override void _Ready()
    {
        GetNode<SignalBus>("/root/SignalBus").ClearCells += UpdateOccupiedCells; 
    }

    public Vector2I WorldToCell(Vector2 worldPosition)
    {
        return new Vector2I(
            Mathf.FloorToInt(worldPosition.X / CellSize.X),
            Mathf.FloorToInt(worldPosition.Y / CellSize.Y)
        );
    }

    public Vector2 CellToWorld(Vector2I cell, bool centered = false)
    {
        Vector2 position = new Vector2(cell.X * CellSize.X, cell.Y * CellSize.Y);
        if (centered)
        {
            position += new Vector2(CellSize.X, CellSize.Y) / 2;
        }
        return position;
    }

    public bool IsInBounds(Vector2I cell)
    {
        return cell.X >= BoundsMinCell.X &&
               cell.Y >= BoundsMinCell.Y &&
               cell.X <= BoundsMaxCell.X &&
               cell.Y <= BoundsMaxCell.Y;
    }

    public bool CanPlace(GridPlaceable placeable, Vector2I anchorCell)
    {
        if (placeable == null)
        {
            return false;
        }

        Vector2I previousAnchor = placeable.def.AnchorCell;
        placeable.def.AnchorCell = anchorCell;

        Array<Vector2I> cells = placeable.def.GetOccupiedCells();
        foreach (Vector2I cell in cells)
        {
            if (!IsInBounds(cell))
            {
                placeable.def.AnchorCell = previousAnchor;
                return false;
            }
            if (occupiedCells.TryGetValue(cell, out PlaceableDefinition existing) && existing != placeable.def)
            {
                placeable.def.AnchorCell = previousAnchor;
                return false;
            }
        }

        placeable.def.AnchorCell = previousAnchor;
        return true;
    }

    private void UpdateOccupiedCells()
    {
        RunState runstate = GetNode<RunState>("/root/RunState");
        occupiedCells.Clear();
        foreach (PlaceableDefinition def in runstate.ActivePlaceables)
        {
            foreach (Vector2I cell in def.GetOccupiedCells())
            {
                occupiedCells[cell] = def;
            }
        }
    }

    public bool TryPlace(GridPlaceable placeable, Vector2I anchorCell)
    {
        if (!CanPlace(placeable, anchorCell))
        {
            return false;
        }
        placeable.def.AnchorCell = anchorCell;
        placeable.GlobalPosition = CellToWorld(anchorCell, placeable.def is UnitDefinition);
        GetNode<SignalBus>("/root/SignalBus").PublishPlaceablePlaced(placeable.def);
        UpdateOccupiedCells();
        return true;
    }
}
