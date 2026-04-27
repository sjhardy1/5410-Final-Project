using Godot;
public partial class AttackState : ICombatantState
{
    private Combatant combatant;
    private ITargetable target;
    private float attackTimer = 0f;

    public AttackState(Combatant combatant, ITargetable target)
    {
        this.combatant = combatant;
        this.target = target;
        target.Defeated += OnTargetDefeated;
    }

    public void Enter()
    {
        combatant.LinearVelocity = Vector2.Zero;
        attackTimer = combatant.OffensiveAttributes.AttackCooldown / 2;
        if(combatant.childScene.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D") is AnimatedSprite2D sprite) sprite.Play("attack");
    }

    public void Exit()
    {
        target.Defeated -= OnTargetDefeated;
    }

    public void Process(double delta)
    {
        if(target == null)
        {
            combatant.ChangeState(new IdleState(combatant));
            return;
        }
         // If target is out of range, transition back to ChaseState
        if (!CombatRangeUtility.IsTargetInAttackRange(combatant, target))
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
        
        GD.Print(combatant.CoreAttributes.DisplayName + "#" + combatant.uid + " defeated " + target.CoreAttributes.DisplayName + "#" + target.uid + ", now changing to idle state.");
        combatant.ChangeState(new IdleState(combatant));
    }
}