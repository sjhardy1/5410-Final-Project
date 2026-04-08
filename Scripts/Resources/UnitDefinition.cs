using Godot;
using Godot.Collections;

[GlobalClass]
public partial class UnitDefinition : LootDefinitionModel
{
    [Export] public override CoreAttributes CoreAttributes { get; set; }
    [Export] public override LootAttributes LootAttributes { get; set; }
    [Export] public DefensiveAttributes DefensiveAttributes { get; set; }
    [Export] public OffensiveAttributes OffensiveAttributes { get; set; }
    [Export] public float UpkeepFoodPerRound { get; set; } = 0f;
    [Export] public PackedScene Scene { get; set; }
}