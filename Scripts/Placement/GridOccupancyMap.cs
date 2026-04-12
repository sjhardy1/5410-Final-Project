using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class GridOccupancyMap : Node
{
    [Export] public Vector2I CellSize { get; set; } = new Vector2I(64, 64);
    [Export] public Vector2I BoundsMinCell { get; set; } = new Vector2I(-10, -10);
    [Export] public Vector2I BoundsMaxCell { get; set; } = new Vector2I(10, 10);

    private readonly System.Collections.Generic.Dictionary<Vector2I, GridPlaceable> occupiedCells = new();

    public Vector2I WorldToCell(Vector2 worldPosition)
    {
        return new Vector2I(
            Mathf.FloorToInt(worldPosition.X / CellSize.X),
            Mathf.FloorToInt(worldPosition.Y / CellSize.Y)
        );
    }

    public Vector2 CellToWorld(Vector2I cell)
    {
        return new Vector2(cell.X * CellSize.X, cell.Y * CellSize.Y);
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

    public bool TryPlace(GridPlaceable placeable, Vector2I anchorCell)
    {
        if (!CanPlace(placeable, anchorCell))
        {
            return false;
        }

        Remove(placeable);

        placeable.AnchorCell = anchorCell;
        foreach (Vector2I cell in placeable.GetOccupiedCells())
        {
            occupiedCells[cell] = placeable;
        }
        if(placeable.lootType == LootType.Building)
        {
            GetNode<SignalBus>("/root/SignalBus").PublishBuildingPlaced(placeable.id, CellToWorld(anchorCell));
        } else if (placeable.lootType == LootType.Unit)
        {
            GetNode<SignalBus>("/root/SignalBus").PublishUnitPlaced(placeable.id, CellToWorld(anchorCell));
        }
        placeable.GlobalPosition = CellToWorld(anchorCell);
        return true;
    }

    public void Remove(GridPlaceable placeable)
    {
        if (placeable == null)
        {
            return;
        }

        var toClear = new List<Vector2I>();
        foreach (KeyValuePair<Vector2I, GridPlaceable> kvp in occupiedCells)
        {
            if (kvp.Value == placeable)
            {
                toClear.Add(kvp.Key);
            }
        }

        foreach (Vector2I cell in toClear)
        {
            occupiedCells.Remove(cell);
        }
    }
}
