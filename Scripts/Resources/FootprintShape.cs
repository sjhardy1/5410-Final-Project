using Godot;
using Godot.Collections;

[GlobalClass]
public partial class FootprintShape : Resource
{
    [Export]
    public Array<Vector2I> OccupiedOffsets { get; set; } = new Array<Vector2I> { Vector2I.Zero };

    public Array<Vector2I> GetOffsets()
    {
        var cells = new Array<Vector2I>();
        foreach (Vector2I offset in OccupiedOffsets)
        {
            cells.Add(offset);
        }
        return cells;
    }
}
