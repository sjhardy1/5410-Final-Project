using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class GridOccupancyMap : Node2D
{
    [Export] public Vector2I CellSize { get; set; } = new Vector2I(64, 64);
    [Export] public Vector2I BoundsMinCell { get; set; } = new Vector2I(-10, -10);
    [Export] public Vector2I BoundsMaxCell { get; set; } = new Vector2I(10, 10);

    private TileMapLayer tiles;
    private readonly System.Collections.Generic.Dictionary<Vector2I, GridPlaceable> occupiedCells = new();

    public override void _Ready()
    {
        GetNode<SignalBus>("/root/SignalBus").ClearCells += UpdateOccupiedCells; 
        tiles = GetNode<TileMapLayer>("Tiles"); 
    }
    public override void _ExitTree()
    {
        GetNode<SignalBus>("/root/SignalBus").ClearCells -= UpdateOccupiedCells;
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

        Vector2I previousAnchor = placeable.AnchorCell;
        placeable.AnchorCell = anchorCell;

        Array<Vector2I> cells = placeable.GetOccupiedCells();
        foreach (Vector2I cell in cells)
        {
            if (!IsInBounds(cell))
            {
                placeable.AnchorCell = previousAnchor;
                return false;
            }
            if (occupiedCells.TryGetValue(cell, out GridPlaceable existing) && existing != placeable)
            {
                placeable.AnchorCell = previousAnchor;
                return false;
            }
        }

        placeable.AnchorCell = previousAnchor;
        return true;
    }

    private void UpdateOccupiedCells()
    {
        RunState runstate = GetNode<RunState>("/root/RunState");
        occupiedCells.Clear();
        //tiles.Clear();
        foreach (GridPlaceable placeable in runstate.ActivePlaceables)
        {
            foreach (Vector2I cell in placeable.GetOccupiedCells())
            {
                occupiedCells[cell] = placeable;
                //tiles.SetCell(cell, 1, Vector2I.Zero);
            }
        }
    }

    public bool TryPlace(GridPlaceable placeable, Vector2I anchorCell)
    {
        if (!CanPlace(placeable, anchorCell))
        {
            return false;
        }
        placeable.AnchorCell = anchorCell;
        placeable.GlobalPosition = CellToWorld(anchorCell, placeable.def is UnitDefinition);
        placeable.ZIndex = (int)(placeable.GlobalPosition.Y / CellSize.Y) + 10;
        GetNode<SignalBus>("/root/SignalBus").PublishPlaceablePlaced(placeable);
        UpdateOccupiedCells();
        return true;
    }
}
