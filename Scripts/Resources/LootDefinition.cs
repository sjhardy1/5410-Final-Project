using System.Numerics;
using System.Xml;
using Godot;

public abstract partial class LootDefinition : Resource
{
    public virtual CoreAttributes CoreAttributes { get; set; }
    public virtual LootAttributes LootAttributes { get; set; }
    public virtual LootType LootType { get; }
    public virtual PackedScene Scene { get; set; }
    public int Uid { get; set; }
}