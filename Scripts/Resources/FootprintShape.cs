using Godot;
using Godot.Collections;

[GlobalClass]
public partial class FootprintShape : Resource
{
    [Export]
    public Array<Vector2I> OccupiedOffsets { get; set; } = new Array<Vector2I> { Vector2I.Zero };

    public Array<Vector2I> GetOffsetsForRotation(int quarterTurnsClockwise)
    {
        int turns = Mathf.PosMod(quarterTurnsClockwise, 4);
        var rotated = new Array<Vector2I>();

        foreach (Vector2I offset in OccupiedOffsets)
        {
            Vector2I current = offset;
            for (int i = 0; i < turns; i++)
            {
                current = new Vector2I(current.Y, -current.X);
            }
            rotated.Add(current);
        }

        return rotated;
    }
}
