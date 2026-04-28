using Godot;
public partial class AttackState : ICombatantState
{
    private Combatant combatant;
    private ITargetable target;

    public AttackState(Combatant combatant, ITargetable target)
    {
        this.combatant = combatant;
        this.target = target;
        target.Defeated += OnTargetDefeated;
    }

    public void Enter()
    {
        combatant.LinearVelocity = Vector2.Zero;
    }

    public void Exit()
    {
        target.Defeated -= OnTargetDefeated;
    }

    public void Process(double delta)
    {
        if(target == null || !target.IsInsideTree())
        {
            combatant.ChangeState(new IdleState(combatant));
            return;
        }
        if (combatant.attackCooldownTimer >= combatant.OffensiveAttributes.AttackCooldown)
        {
            if(combatant.childScene.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D") is AnimatedSprite2D sprite) sprite.Play("attack");
            combatant.PerformAttack(target);
            combatant.attackCooldownTimer = 0f; // Reset timer for next attack
        }
         // If target is out of range, transition back to ChaseState
        if (!CombatRangeUtility.IsTargetInAttackRange(combatant, target))
        {
            combatant.ChangeState(new ChaseState(combatant, target));
            return;
        }
    }

    private void OnTargetDefeated()
    {
        
        GD.Print(combatant.CoreAttributes.DisplayName + "#" + combatant.uid + " defeated " + target.CoreAttributes.DisplayName + "#" + target.uid + ", now changing to idle state.");
        combatant.ChangeState(new IdleState(combatant));
    }
}