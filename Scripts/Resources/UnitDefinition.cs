using Godot;
using Godot.Collections;

[GlobalClass]
public partial class UnitDefinition : PlaceableDefinition
{
    [Export] public override CoreAttributes CoreAttributes { get; set; }
    [Export] public override LootAttributes LootAttributes { get; set; }
    [Export] public override FootprintShape Footprint { get; set; }
    [Export] public override DefensiveAttributes DefensiveAttributes { get; set; }
    [Export] public OffensiveAttributes OffensiveAttributes { get; set; }
    [Export] public float moveSpeed { get; set; } = 100;
    [Export] public override int UpkeepFoodPerRound { get; set; } = 0;
    [Export] public override PackedScene Scene { get; set; }
}