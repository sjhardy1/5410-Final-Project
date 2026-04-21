using System;
using Godot;
using System.Collections.Generic;
public partial class IdleState : ICombatantState
{
    private Combatant combatant;
    public IdleState(Combatant combatant)
    {
        this.combatant = combatant;
    }

    public void Enter()
    {
        if(combatant.childScene.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D") is AnimatedSprite2D sprite) sprite.Play("default");
    }

    public void Exit()
    {
        // No cleanup needed for idle state
    }

    public void Process(double delta)
    {
        ITargetable target = combatant.FindTarget();
        if (target != null)
        {
            combatant.ChangeState(new ChaseState(combatant, target));
        }
    }
}