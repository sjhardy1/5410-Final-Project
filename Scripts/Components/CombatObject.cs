using Godot;
using System;

public partial class CombatObject : Node2D, ITargetable
{
    public CoreAttributes CoreAttributes { get; set; }
    public DefensiveAttributes DefensiveAttributes { get; set; }
    public PackedScene scene { get; set; }
    public Node2D childScene;
    public Faction faction;
    public int uid;
    private HealthBar healthBar;
    [Signal] public delegate void DefeatedSignalEventHandler();
    public event Action Defeated;

    public CombatObject(BuildingDefinition def)
    {
        CoreAttributes = def.CoreAttributes;
        DefensiveAttributes = def.DefensiveAttributes.Duplicate() as DefensiveAttributes;
        if (DefensiveAttributes != null)
        {
            DefensiveAttributes.Health = DefensiveAttributes.MaxHealth;
        }
        scene = def.Scene;
        faction = Faction.Ally;
    }

    public override void _Ready()
    {
        childScene = scene.Instantiate<Node2D>();
        AddChild(childScene);

        healthBar = new HealthBar();
        healthBar.Position = new Vector2(0f, -40f);
        healthBar.SetAttributes(DefensiveAttributes);
        AddChild(healthBar);
    }
    public void TakeDamage(float damage)
    {
        DefensiveAttributes.Health -= damage;
        if (DefensiveAttributes.Health <= 0)
        {
            Die();
        }
    }
    public void Die()
    {
        GetNode<SignalBus>("/root/SignalBus").PublishBuildingDestroyed(uid);
        Defeated?.Invoke();
        EmitSignal(SignalName.DefeatedSignal);
        QueueFree();
    }
}
