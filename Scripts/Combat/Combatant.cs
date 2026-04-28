using Godot;
using System;

public partial class Combatant : RigidBody2D, ITargetable
{
    private const float TileSize = 64f;
    private const float CollisionRadius = TileSize / 2.5f;
    private PackedScene SlashScene = GD.Load<PackedScene>("res://Scenes/FX/Slash.tscn");
    private PackedScene ArrowScene = GD.Load<PackedScene>("res://Scenes/FX/Arrow.tscn");
    private PackedScene DarkOrbScene = GD.Load<PackedScene>("res://Scenes/FX/DarkOrb.tscn");
    public float attackCooldownTimer = 0f;
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


    public void Process(double delta)
    {
        LinearVelocity *= 0.95f; // Apply friction
        ZIndex = (int)(Position.Y / TileSize) + 10;
        attackCooldownTimer += (float)delta;
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
        moveSpeed = definition.moveSpeed;
        Setup();
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
        moveSpeed = definition.moveSpeed;
        Setup();
    }
    private void Setup()
    {
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
        
        Timer timer = new Timer();
        timer.WaitTime = DoAttackAnimation(target); // Delay the damage application to sync with animation
        timer.Autostart = true;
        AddChild(timer);
        timer.Timeout += () => {
            try
            {
                ApplyAttack(target);
            }
            catch (ObjectDisposedException){}
            timer.QueueFree();
        };
    }
    public float DoAttackAnimation(ITargetable target)
    {
        Vector2 direction = Position.DirectionTo(target.Position);
        float distance = Position.DistanceTo(target.Position);
        float wait = 0.25f;
        if(CoreAttributes.Id == "archer")
        {
            return CreateProjectileAnimation(1500f, 0.5f, distance, target.Position, "arrow");
        }
        if(CoreAttributes.Id == "sharpshooter")
        {
            return CreateProjectileAnimation(3000f, 1f, distance, target.Position, "arrow");
        }
        if(CoreAttributes.Id == "black_mage")
        {
            return CreateProjectileAnimation(400f, 0f, distance, target.Position, "dark_orb");
        }
        if(faction == Faction.Enemy)
        {
            AnimatedSprite2D slashInstance = SlashScene.Instantiate<AnimatedSprite2D>();
            AddChild(slashInstance);
            slashInstance.AnimationLooped += slashInstance.QueueFree;
            slashInstance.Rotation += direction.Angle();
            slashInstance.Position = direction * (distance - 64);
            RunState runState = GetNode<RunState>("/root/RunState");
            runState.sfxPlayer.Stream = runState.audioCache["slash"];
            runState.sfxPlayer.Play();
        }
        return wait;
    }
    private float CreateProjectileAnimation(float speed, float chargeDelay, float distance, Vector2 targetPosition, string type)
    {
        PackedScene projectileScene = type switch
        {
            "arrow" => ArrowScene,
            "dark_orb" => DarkOrbScene,
            _ => SlashScene
        };
        Projectile projectileInstance = projectileScene.Instantiate<Projectile>();
        float liveTime = distance / speed;
        projectileInstance.Initialize(targetPosition, speed, liveTime);
        if(chargeDelay > 0f)
        {
            Timer timer = new Timer();
            timer.WaitTime = chargeDelay;
            timer.Autostart = true;
            timer.Timeout += () => {
                RunState runState = GetNode<RunState>("/root/RunState");
                runState.sfxPlayer.Stream = runState.audioCache[type == "arrow" ? "arrow" : "magic"];
                runState.sfxPlayer.Play();
                AddChild(projectileInstance);
                timer.QueueFree();
            };
            AddChild(timer);
        } else
        {
            AddChild(projectileInstance);
        }
        return liveTime + chargeDelay;
    }
    public void ApplyAttack(ITargetable target)
    {
        if(!target.IsInsideTree()) return;
        if(target is Combatant combatant){
            combatant.ApplyCentralImpulse(Position.DirectionTo(target.Position) * 256f * OffensiveAttributes.KnockbackCoefficient / target.DefensiveAttributes.WeightCoefficient); 
        }
        float damage = OffensiveAttributes.AttackDamage - target.DefensiveAttributes.Armor / 2;
        if (damage < 0) damage = 0; // Prevent healing from negative damage
        target.TakeDamage(damage);
    }
    public void TakeDamage(float damage)
    {
        DefensiveAttributes.Health -= damage * (1f - DefensiveAttributes.DamageReductionPercent / 100f);
        if (DefensiveAttributes.Health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        // Emit signal to notify GameRoot of death
        GetNode<SignalBus>("/root/SignalBus").PublishUnitDied(uid);
        Defeated?.Invoke();
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