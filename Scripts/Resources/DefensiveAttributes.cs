using Godot;
[GlobalClass]
public partial class DefensiveAttributes : Resource
{
    [Export] public float Health { get; set; } = 50f;
    [Export] public float Armor { get; set; } = 0f;
    [Export] public float DamageReductionPercent { get; set; } = 0f;
    [Export] public float HealthRegenPerRound { get; set; } = 0f;
}