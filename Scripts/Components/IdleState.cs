using System;
using Godot;
using System.Collections.Generic;
public partial class IdleState : ICombatantState
{
    private Combatant combatant;
    private RunState runState;
    public IdleState(Combatant combatant)
    {
        this.combatant = combatant;
    }

    public void Enter(RunState runState)
    {
        this.runState = runState;
        if(combatant.childScene.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D") is AnimatedSprite2D sprite) sprite.Play("default");
    }

    public void Exit()
    {
        // No cleanup needed for idle state
    }

    public void Process(double delta)
    {
        // In idle state, the combatant does nothing
        List<Combatant> combatants = runState.ActiveCombatants;
        Combatant target = null;
        float closestDistance = float.MaxValue;
        foreach (Combatant other in combatants)
        {
            if (other != combatant && other.faction != combatant.faction)
            {
                // Check if the other combatant is within detection range
                if (combatant.Position.DistanceTo(other.Position) < 20 * 64 &&
                    combatant.Position.DistanceTo(other.Position) < closestDistance)
                {
                    closestDistance = combatant.Position.DistanceTo(other.Position);
                    target = other;
                }
            }
        }
        if (target != null)
        {
            combatant.ChangeState(new ChaseState(combatant, target));
        }
    }
}