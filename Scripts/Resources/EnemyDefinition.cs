using System.Text.RegularExpressions;
using Godot;

[GlobalClass]
public partial class EnemyDefinition : Resource
{
    [Export] public CoreAttributes CoreAttributes { get; set; }
    [Export] public DefensiveAttributes DefensiveAttributes { get; set; }
    [Export] public OffensiveAttributes OffensiveAttributes { get; set; }
    [Export] public int Value { get; set; }
    [Export] public PackedScene Scene { get; set; }
}