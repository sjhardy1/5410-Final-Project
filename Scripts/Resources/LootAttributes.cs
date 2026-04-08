using Godot;
public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
[GlobalClass]
public partial class LootAttributes : Resource
{
    [Export] public int UnlockWave { get; set; } = 1;
    [Export] public Rarity Rarity { get; set; } = Rarity.Common;
}