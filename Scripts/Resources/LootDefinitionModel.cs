using Godot;
public enum LootType
{
    Unit,
    Building, 
    Technology
}
public abstract partial class LootDefinitionModel : Resource
{
    public virtual CoreAttributes CoreAttributes { get; set; }
    public virtual LootAttributes LootAttributes { get; set; }
    public virtual LootType LootType { get; }
}