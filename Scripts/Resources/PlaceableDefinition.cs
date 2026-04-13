using Godot;
using Godot.Collections;

public abstract partial class PlaceableDefinition : LootDefinition
{
    public virtual FootprintShape Footprint { get; set; }
    public virtual int UpkeepFoodPerRound { get; set; }
    public virtual DefensiveAttributes DefensiveAttributes { get; set; }
    public Vector2I AnchorCell { get; set; }
    public Array<Vector2I> GetOccupiedCells()
    {
        var occupied = new Array<Vector2I>();
        if (Footprint == null)
        {
            occupied.Add(AnchorCell);
            return occupied;
        }

        Array<Vector2I> offsets = Footprint.GetOffsets();
        foreach (Vector2I offset in offsets)
        {
            occupied.Add(AnchorCell + offset);
        }

        return occupied;
    }
}