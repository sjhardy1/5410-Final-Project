using Godot;
[GlobalClass]
public partial class DefensiveAttributes : Resource
{
    [Export] public float MaxHealth { get; set; } = 50f;
    public float Health { get; set; }
    [Export] public float Armor { get; set; } = 0f;
    [Export] public float DamageReductionPercent { get; set; } = 0f;
    [Export] public float WeightCoefficient { get; set; } = 1f;
}