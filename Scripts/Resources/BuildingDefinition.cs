using Godot;

[GlobalClass]
public partial class BuildingDefinition : LootDefinitionModel
{
    [Export] public override CoreAttributes CoreAttributes { get; set; }
    [Export] public override LootAttributes LootAttributes { get; set; }
    [Export] public DefensiveAttributes DefensiveAttributes { get; set; }
    [Export] public float UpkeepFoodPerRound { get; set; } = 0f;
    [Export] public float UpkeepWoodPerRound { get; set; } = 0f;
    [Export] public PackedScene Scene { get; set; }
    [Export] public float ProductionFoodPerRound { get; set; } = 0f;
    [Export] public float ProductionWoodPerRound { get; set; } = 0f;
    public override LootType LootType => LootType.Building;
}