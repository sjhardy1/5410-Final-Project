using Godot;
using Godot.Collections;

public partial class ChaseState : ICombatantState
{
    private const float ProbeLength = 96f;
    private const float ProbeWidth = 48f;
    private const float ProbeForwardOffset = 48f;
    private const float AngleStepDegrees = 10f;
    private const float MaxScanDegrees = 180f;
    private const float AvoidanceCommitDuration = 1.0f;

    private Combatant combatant;
    private ITargetable target;
    private Vector2 committedDirection;
    private float committedSteerTimeRemaining;

    public ChaseState(Combatant combatant, ITargetable target)
    {
        this.combatant = combatant;
        this.target = target;
        target.Defeated += OnTargetDefeated;
    }

    public void Enter()
    {
        if(combatant.childScene.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D") is AnimatedSprite2D sprite) {
            sprite.Play("run");
            sprite.SpeedScale = 1f;
        }
    }

    public void Exit()
    {
        target.Defeated -= OnTargetDefeated;
        combatant.LinearVelocity *= 0.2f;
    }

    public void Process(double delta)
    {
        float deltaF = (float)delta;
        committedSteerTimeRemaining = Mathf.Max(0f, committedSteerTimeRemaining - deltaF);

        // Move towards the target
        if (target != null)
        {
            ITargetable newTarget = combatant.FindTarget();
            if (newTarget != null && newTarget != target)
            {
                target.Defeated -= OnTargetDefeated;
                target = newTarget;
                target.Defeated += OnTargetDefeated;
            }

            Vector2 desiredDirection = (target.Position - combatant.Position).Normalized();
            if (TryGetSteeringDirection(desiredDirection, out Vector2 steeringDirection))
            {
                combatant.LinearVelocity += steeringDirection * combatant.moveSpeed * deltaF * 4f;
            }
            if(desiredDirection.X < 0){
                if(combatant.childScene.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D") is AnimatedSprite2D sprite) sprite.FlipH = true;
                if(combatant.childScene.GetNodeOrNull<Sprite2D>("Sprite2D") is Sprite2D sprite2) sprite2.FlipH = true;
            } else {
                if(combatant.childScene.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D") is AnimatedSprite2D sprite) sprite.FlipH = false;
                if(combatant.childScene.GetNodeOrNull<Sprite2D>("Sprite2D") is Sprite2D sprite2) sprite2.FlipH = false;
            }
        }
        else
        {
            combatant.LinearVelocity = Vector2.Zero;
        }
        
        // If within attack range, transition to AttackState
        if (target != null && CombatRangeUtility.IsTargetInAttackRange(combatant, target))
        {
            combatant.ChangeState(new AttackState(combatant, target));
        }
    }

    private bool TryGetSteeringDirection(Vector2 desiredDirection, out Vector2 steeringDirection)
    {
        if (committedSteerTimeRemaining > 0f && committedDirection != Vector2.Zero && IsProbeDirectionFree(committedDirection))
        {
            steeringDirection = committedDirection;
            return true;
        }

        bool pathBlockedAhead = !IsProbeDirectionFree(desiredDirection);
        if (!pathBlockedAhead)
        {
            committedDirection = Vector2.Zero;
            steeringDirection = desiredDirection;
            return true;
        }

        if (FindFreeDirection(desiredDirection, out Vector2 freeDirection))
        {
            committedSteerTimeRemaining = AvoidanceCommitDuration;
            committedDirection = freeDirection;
            steeringDirection = freeDirection;
            return true;
        }

        committedDirection = Vector2.Zero;
        steeringDirection = Vector2.Zero;
        return false;
    }

    private bool FindFreeDirection(Vector2 desiredDirection, out Vector2 freeDirection)
    {
        int maxSteps = Mathf.CeilToInt(MaxScanDegrees / AngleStepDegrees);
        for (int step = 0; step <= maxSteps; step++)
        {
            foreach (float signedMultiplier in GetStepSigns(step))
            {
                float offsetDegrees = step * AngleStepDegrees * signedMultiplier;
                Vector2 candidateDirection = desiredDirection.Rotated(Mathf.DegToRad(offsetDegrees)).Normalized();
                if (IsProbeDirectionFree(candidateDirection))
                {
                    freeDirection = candidateDirection;
                    return true;
                }
            }
        }

        freeDirection = Vector2.Zero;
        return false;
    }

    private Array<float> GetStepSigns(int step)
    {
        if (step == 0)
        {
            return new Array<float> { 0f };
        }

        return new Array<float> { -1f, 1f };
    }

    private bool IsProbeDirectionFree(Vector2 direction)
    {
        RectangleShape2D probeShape = new RectangleShape2D { Size = new Vector2(ProbeLength, ProbeWidth) };
        Vector2 probeCenter = combatant.GlobalPosition + direction * ProbeForwardOffset;
        Transform2D probeTransform = new Transform2D(direction.Angle(), probeCenter);
        PhysicsShapeQueryParameters2D query = new PhysicsShapeQueryParameters2D
        {
            Shape = probeShape,
            Transform = probeTransform,
            CollideWithAreas = false,
            CollideWithBodies = true,
            Exclude = new Array<Rid> { combatant.GetRid() }
        };

        Array<Dictionary> hits = combatant.GetWorld2D().DirectSpaceState.IntersectShape(query, 16);
        foreach (Dictionary hit in hits)
        {
            if (!hit.ContainsKey("collider"))
            {
                continue;
            }

            GodotObject collider = hit["collider"].AsGodotObject();
            if (ReferenceEquals(collider, target as GodotObject))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private void OnTargetDefeated()
    {
        combatant.ChangeState(new IdleState(combatant));
    }
}