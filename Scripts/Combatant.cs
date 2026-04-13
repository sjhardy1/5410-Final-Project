using Godot;

public partial class Combatant : Node2D
{
    public CoreAttributes CoreAttributes { get; set; }
    public DefensiveAttributes DefensiveAttributes { get; set; }
    public OffensiveAttributes OffensiveAttributes { get; set; }
    public PackedScene Scene { get; set; }
    public Node2D childScene;
    private ICombatantState currentState;
    public Faction faction;
    public int uid;
    [Signal] public delegate void DefeatedEventHandler();

    public override void _Process(double delta)
    {
        currentState.Process(delta);
    }   
    public Combatant(UnitDefinition definition)
    {
        CoreAttributes = definition.CoreAttributes;
        DefensiveAttributes = definition.DefensiveAttributes;
        OffensiveAttributes = definition.OffensiveAttributes;
        Scene = definition.Scene;
        faction = Faction.Ally;
    }
    public Combatant(EnemyDefinition definition, Vector2 position)
    {
        CoreAttributes = definition.CoreAttributes;
        DefensiveAttributes = definition.DefensiveAttributes;
        OffensiveAttributes = definition.OffensiveAttributes;
        Scene = definition.Scene;
        Position = position;
        faction = Faction.Enemy;
    }
    public override void _Ready()
    {
        childScene = Scene.Instantiate<Node2D>();
        AddChild(childScene);
        ChangeState(new IdleState(this));
    }
    public void ChangeState(ICombatantState newState)
    {
        if(currentState != null) currentState.Exit();
        currentState = newState;
        currentState.Enter(GetNode<RunState>("/root/RunState"));
    }
    public void PerformAttack(Combatant target)
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
            Die();
        }
    }
    public void Die()
    {
        // Emit signal to notify GameRoot of death
        GetNode<SignalBus>("/root/SignalBus").PublishUnitDied(uid);
        EmitSignal(nameof(Defeated));
        QueueFree(); // Remove from scene
    }
}