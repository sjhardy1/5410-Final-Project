using Godot;
using Godot.Collections;

[GlobalClass]
public partial class UnitDefinition : LootDefinitionModel
{
    [Export] public override CoreAttributes CoreAttributes { get; set; }
    [Export] public override LootAttributes LootAttributes { get; set; }
    [Export] public override FootprintShape Footprint { get; set; }
    [Export] public DefensiveAttributes DefensiveAttributes { get; set; }
    [Export] public OffensiveAttributes OffensiveAttributes { get; set; }
    [Export] public int UpkeepFoodPerRound { get; set; } = 0;
    [Export] public override PackedScene Scene { get; set; }
}