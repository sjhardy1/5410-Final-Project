using Godot;
using System;

public partial class CombatObject : StaticBody2D, ITargetable
{
    private const float TileSize = 64f;
    private const float CollisionRadius = TileSize / 2.5f;
    public CoreAttributes CoreAttributes { get; set; }
    public DefensiveAttributes DefensiveAttributes { get; set; }
    public PackedScene scene { get; set; }
    public Node2D childScene;
    public Faction faction;
    public int uid { get; set; }
    private HealthBar healthBar;
    private FootprintShape footprint;
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
        footprint = def.Footprint;
        faction = Faction.Ally;
    }

    public override void _Ready()
    {
        childScene = scene.Instantiate<Node2D>();
        AddChild(childScene);

        AddFootprintCollisions(footprint);

        healthBar = new HealthBar();
        healthBar.Position = new Vector2(0f, -40f);
        healthBar.SetAttributes(DefensiveAttributes);
        AddChild(healthBar);
    }
    public override void _ExitTree()
    {
        GetNode<RunState>("/root/RunState").ActiveObjects.Remove(this);
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

    private void AddFootprintCollisions(FootprintShape shape)
    {
        if (shape == null)
        {
            AddSquareCollision(Vector2.One * CollisionRadius);
            return;
        }

        foreach (Vector2I tileOffset in shape.GetOffsets())
        {
            Vector2 localCenter = new Vector2(tileOffset.X * TileSize, tileOffset.Y * TileSize) + Vector2.One * CollisionRadius;
            AddSquareCollision(localCenter);
        }
    }

    private void AddSquareCollision(Vector2 localCenter)
    {
        RectangleShape2D shape = new RectangleShape2D { Size = new Vector2(TileSize, TileSize) };
        CollisionShape2D collisionShape = new CollisionShape2D
        {
            Shape = shape,
            Position = localCenter
        };
        AddChild(collisionShape);
    }
}
