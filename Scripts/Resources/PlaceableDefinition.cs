using Godot;

public abstract partial class PlaceableDefinition : LootDefinition
{
    public virtual FootprintShape Footprint { get; set; }
    public virtual int UpkeepFoodPerRound { get; set; }
    public virtual DefensiveAttributes DefensiveAttributes { get; set; }
    public Vector2I AnchorCell { get; set; }
}