using Godot;
[GlobalClass]
public partial class OffensiveAttributes : Resource
{
    [Export] public float AttackDamage { get; set; } = 1f;
    [Export] public float AttackRange { get; set; } = 0f;
    [Export] public float AttackCooldown { get; set; } = 1.0f;
}