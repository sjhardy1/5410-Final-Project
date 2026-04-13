using Godot;
public partial class AttackState : ICombatantState
{
    private Combatant combatant;
    private Combatant target;
    private RunState runState;
    private float attackTimer = 0f;

    public AttackState(Combatant combatant, Combatant target)
    {
        this.combatant = combatant;
        this.target = target;
        target.Defeated += OnTargetDefeated;
    }

    public void Enter(RunState runState)
    {
        this.runState = runState;
        attackTimer = 0f;
        if(combatant.childScene.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D") is AnimatedSprite2D sprite) sprite.Play("attack");
    }

    public void Exit()
    {
        // No cleanup needed for now.
    }

    public void Process(double delta)
    {
        if(target == null)
        {
            combatant.ChangeState(new IdleState(combatant));
            return;
        }
         // If target is out of range, transition back to ChaseState
        if (combatant.Position.DistanceTo(target.Position) > combatant.OffensiveAttributes.AttackRange * 64)
        {
            combatant.ChangeState(new ChaseState(combatant, target));
            return;
        }
        attackTimer += (float)delta;
        if (attackTimer >= combatant.OffensiveAttributes.AttackCooldown)
        {
            combatant.PerformAttack(target);
            attackTimer = 0f; // Reset timer for next attack
        }
    }
    private void OnTargetDefeated()
    {
        target.Defeated -= OnTargetDefeated;
        target = null;
        combatant.ChangeState(new IdleState(combatant));
    }
}