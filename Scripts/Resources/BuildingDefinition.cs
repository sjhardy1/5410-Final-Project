using Godot;

[GlobalClass]
public partial class BuildingDefinition : LootDefinitionModel
{
    [Export] public override CoreAttributes CoreAttributes { get; set; }
    [Export] public override LootAttributes LootAttributes { get; set; }
    [Export] public override FootprintShape Footprint { get; set; }
    [Export] public DefensiveAttributes DefensiveAttributes { get; set; }
    [Export] public int UpkeepFoodPerRound { get; set; } = 0;
    [Export] public int UpkeepWoodPerRound { get; set; } = 0;
    [Export] public override PackedScene Scene { get; set; }
    [Export] public int ProductionFoodPerRound { get; set; } = 0;
    [Export] public int ProductionWoodPerRound { get; set; } = 0;
    public override LootType LootType => LootType.Building;
}