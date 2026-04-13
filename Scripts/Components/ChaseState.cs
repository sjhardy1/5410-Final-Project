using Godot;

public partial class ChaseState : ICombatantState
{
    private Combatant combatant;
    private Combatant target;
    private RunState runState;

    public ChaseState(Combatant combatant, Combatant target)
    {
        this.combatant = combatant;
        this.target = target;
        target.Defeated += OnTargetDefeated;
    }

    public void Enter(RunState runState)
    {
        this.runState = runState;
        if(combatant.childScene.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D") is AnimatedSprite2D sprite) sprite.Play("run");
    }

    public void Exit()
    {
        // Stop moving
    }

    public void Process(double delta)
    {
        // Move towards the target
        if (target != null)
        {
            Vector2 direction = (target.Position - combatant.Position).Normalized();
            float speed = 100f; // Example speed value
            combatant.Position += direction * speed * (float)delta;
        }
        // If within attack range, transition to AttackState
        if (target != null && combatant.Position.DistanceTo(target.Position) < combatant.OffensiveAttributes.AttackRange * 64) // Example attack range
        {
            combatant.ChangeState(new AttackState(combatant, target));
        }
    }
    private void OnTargetDefeated()
    {
        target.Defeated -= OnTargetDefeated;
        target = null;
        combatant.ChangeState(new IdleState(combatant));
    }
}