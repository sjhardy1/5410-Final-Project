using Godot;
using System;

public partial class Combatant : RigidBody2D, ITargetable
{
    private const float TileSize = 64f;
    private const float CollisionRadius = TileSize / 2f;
    public CoreAttributes CoreAttributes { get; set; }
    public DefensiveAttributes DefensiveAttributes { get; set; }
    public OffensiveAttributes OffensiveAttributes { get; set; }
    public float moveSpeed { get; set; }
    public PackedScene Scene { get; set; }
    public Node2D childScene;
    private ICombatantState currentState;
    public Faction faction;
    public int uid { get; set; }
    private HealthBar healthBar;
    private FootprintShape footprint;
    //[Signal] public delegate void DefeatedSignalEventHandler();
    public event Action Defeated;

    public override void _PhysicsProcess(double delta)
    {
        currentState.Process(delta);
    }   
    public Combatant(UnitDefinition definition)
    {
        CoreAttributes = definition.CoreAttributes;
        DefensiveAttributes = definition.DefensiveAttributes.Duplicate() as DefensiveAttributes;
        if (DefensiveAttributes != null)
        {
            DefensiveAttributes.Health = DefensiveAttributes.MaxHealth;
        }
        OffensiveAttributes = definition.OffensiveAttributes;
        Scene = definition.Scene;
        footprint = definition.Footprint;
        faction = Faction.Ally;
        GravityScale = 0f;
        LockRotation = true;
    }
    public Combatant(EnemyDefinition definition, Vector2 position)
    {
        CoreAttributes = definition.CoreAttributes;
        DefensiveAttributes = definition.DefensiveAttributes.Duplicate() as DefensiveAttributes;
        if (DefensiveAttributes != null)
        {
            DefensiveAttributes.Health = DefensiveAttributes.MaxHealth;
        }
        OffensiveAttributes = definition.OffensiveAttributes;
        Scene = definition.Scene;
        Position = position;
        faction = Faction.Enemy;
        GravityScale = 0f;
        LockRotation = true;
    }
    public override void _Ready()
    {
        childScene = Scene.Instantiate<Node2D>();
        AddChild(childScene);

        if (faction == Faction.Enemy)
        {
            AddCircleCollision(Vector2.Zero);
        }
        else
        {
            AddFootprintCollisions(footprint, true);
        }

        healthBar = new HealthBar();
        healthBar.Position = new Vector2(0f, -40f);
        healthBar.SetAttributes(DefensiveAttributes);
        AddChild(healthBar);
        ChangeState(new IdleState(this));
    }
    public override void _ExitTree()
    {
        if (currentState != null) currentState.Exit();
        currentState = null;
        GD.Print(CoreAttributes.DisplayName + "#" +uid + " is exiting the tree and will be removed from ActiveCombatants.");
        GetNode<RunState>("/root/RunState").ActiveCombatants.Remove(this);
    }
    public void ChangeState(ICombatantState newState)
    {
        if(currentState != null) currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }
    public void PerformAttack(ITargetable target)
    {
        // Simple damage calculation: attacker's attack minus defender's defense
        float damage = OffensiveAttributes.AttackDamage - target.DefensiveAttributes.Armor / 2;
        if (damage < 0) damage = 0; // Prevent healing from negative damage
        target.TakeDamage(damage);
    }
    public void TakeDamage(float damage)
    {
        DefensiveAttributes.Health -= damage;
        if (DefensiveAttributes.Health <= 0)
        {
            if(CoreAttributes.DisplayName == "Warrior") GD.Print("Warrior HP has reached 0, now calling Die() method.");
            Die();
        }
    }

    public void Die()
    {
        // Emit signal to notify GameRoot of death
        GetNode<SignalBus>("/root/SignalBus").PublishUnitDied(uid);
        GD.Print(CoreAttributes.DisplayName + "#" + uid + " has died, now invoking Defeated event.");
        Defeated?.Invoke();
        //EmitSignal(SignalName.DefeatedSignal);
        GD.Print(CoreAttributes.DisplayName + "#" + uid + " died");
        QueueFree(); // Remove from scene
    }
    public ITargetable FindTarget()
    {
        // Simple target acquisition: find the closest enemy combatant
        RunState runState = GetNode<RunState>("/root/RunState");
        ITargetable closestTarget = null;
        float closestDistance = float.MaxValue;
        foreach (Combatant combatant in runState.ActiveCombatants)
        {
            if (combatant.faction != this.faction)
            {
                float distance = Position.DistanceTo(combatant.Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = combatant;
                }
            }
        }
        foreach (CombatObject building in runState.ActiveObjects)
        {
            if (building.faction != this.faction)
            {
                float distance = Position.DistanceTo(building.Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = building;
                }
            }
        }
        return closestTarget;
    }

    private void AddFootprintCollisions(FootprintShape shape, bool centeredOnOrigin)
    {
        if (shape == null)
        {
            AddCircleCollision(centeredOnOrigin ? Vector2.Zero : Vector2.One * CollisionRadius);
            return;
        }

        Vector2 centerOffset = centeredOnOrigin ? Vector2.Zero : Vector2.One * CollisionRadius;
        foreach (Vector2I tileOffset in shape.GetOffsets())
        {
            Vector2 localCenter = new Vector2(tileOffset.X * TileSize, tileOffset.Y * TileSize) + centerOffset;
            AddCircleCollision(localCenter);
        }
    }

    private void AddCircleCollision(Vector2 localCenter)
    {
        CircleShape2D shape = new CircleShape2D { Radius = CollisionRadius };
        CollisionShape2D collisionShape = new CollisionShape2D
        {
            Shape = shape,
            Position = localCenter
        };
        AddChild(collisionShape);
    }
}