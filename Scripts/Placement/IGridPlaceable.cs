using Godot;
using Godot.Collections;

public interface IGridPlaceable
{
    FootprintShape Footprint { get; }
    Vector2I AnchorCell { get; set; }
    int RotationQuarterTurns { get; set; }

    Array<Vector2I> GetOccupiedCells();
}
