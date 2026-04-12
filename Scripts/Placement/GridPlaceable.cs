using Godot;
using Godot.Collections;

public partial class GridPlaceable : Node2D, IGridPlaceable
{
    [Export] public FootprintShape Footprint { get; set; }
    [Export] public bool BlocksMovement { get; set; } = true;

    public Vector2I AnchorCell { get; set; } = Vector2I.Zero;
    public int RotationQuarterTurns { get; set; } = 0;
    public string id { get; set; }
    public LootType lootType { get; set; }

    public Array<Vector2I> GetOccupiedCells()
    {
        var occupied = new Array<Vector2I>();
        if (Footprint == null)
        {
            occupied.Add(AnchorCell);
            return occupied;
        }

        Array<Vector2I> offsets = Footprint.GetOffsetsForRotation(RotationQuarterTurns);
        foreach (Vector2I offset in offsets)
        {
            occupied.Add(AnchorCell + offset);
        }

        return occupied;
    }
    public void Initialize(string id, FootprintShape footprint, LootType lootType)
    {
        this.id = id;
        Footprint = footprint;
        this.lootType = lootType;
    }
}
