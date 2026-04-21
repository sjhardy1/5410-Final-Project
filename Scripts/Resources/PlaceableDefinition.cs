using Godot;
using Godot.Collections;

public abstract partial class PlaceableDefinition : LootDefinition
{
    public virtual FootprintShape Footprint { get; set; }
    public virtual int UpkeepFoodPerRound { get; set; }
    public virtual DefensiveAttributes DefensiveAttributes { get; set; }    
}