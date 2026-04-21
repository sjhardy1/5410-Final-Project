using Godot;
using Godot.Collections;

public partial class ChaseState : ICombatantState
{
    private const float ChaseSpeed = 100f;
    private const float LookAheadDistance = 96f;
    private const float SteerAngleDegrees = 35f;
    private const float AvoidanceWeight = 1.25f;
    private const float SeparationWeight = 1.15f;
    private const float SeparationRadius = 72f;
    private const float MinSeparationDistance = 8f;
    private const float AvoidanceCommitDuration = 0.35f;

    private Combatant combatant;
    private ITargetable target;
    private int committedSteerSign;
    private float committedSteerTimeRemaining;

    public ChaseState(Combatant combatant, ITargetable target)
    {
        this.combatant = combatant;
        this.target = target;
        target.Defeated += OnTargetDefeated;
    }

    public void Enter()
    {
        if(combatant.childScene.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D") is AnimatedSprite2D sprite) sprite.Play("run");
    }

    public void Exit()
    {
        target.Defeated -= OnTargetDefeated;
        combatant.LinearVelocity = Vector2.Zero;
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
            Vector2 steeringDirection = desiredDirection;

            Vector2 obstacleAvoidance = ComputeObstacleAvoidance(desiredDirection);
            if (obstacleAvoidance != Vector2.Zero)
            {
                steeringDirection += obstacleAvoidance * AvoidanceWeight;
            }

            Vector2 separation = ComputeSeparationVector();
            if (separation != Vector2.Zero)
            {
                steeringDirection += separation * SeparationWeight;
            }

            if (steeringDirection == Vector2.Zero)
            {
                steeringDirection = desiredDirection;
            }

            combatant.LinearVelocity = steeringDirection.Normalized() * ChaseSpeed;
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

    private Vector2 ComputeObstacleAvoidance(Vector2 desiredDirection)
    {
        if (desiredDirection == Vector2.Zero)
        {
            return Vector2.Zero;
        }

        if (GetPathClearance(desiredDirection, LookAheadDistance) >= LookAheadDistance)
        {
            if (committedSteerTimeRemaining <= 0f)
            {
                committedSteerSign = 0;
            }
            return Vector2.Zero;
        }

        if (committedSteerSign == 0)
        {
            Vector2 leftDirection = desiredDirection.Rotated(Mathf.DegToRad(-SteerAngleDegrees));
            Vector2 rightDirection = desiredDirection.Rotated(Mathf.DegToRad(SteerAngleDegrees));

            float leftClearance = GetPathClearance(leftDirection, LookAheadDistance);
            float rightClearance = GetPathClearance(rightDirection, LookAheadDistance);

            committedSteerSign = leftClearance >= rightClearance ? -1 : 1;
            committedSteerTimeRemaining = AvoidanceCommitDuration;
        }

        Vector2 preferredDirection = desiredDirection.Rotated(Mathf.DegToRad(committedSteerSign * SteerAngleDegrees));
        if (GetPathClearance(preferredDirection, LookAheadDistance * 0.75f) > 0f)
        {
            return preferredDirection;
        }

        Vector2 oppositeDirection = desiredDirection.Rotated(Mathf.DegToRad(-committedSteerSign * SteerAngleDegrees));
        return oppositeDirection;
    }

    private Vector2 ComputeSeparationVector()
    {
        CircleShape2D searchShape = new CircleShape2D { Radius = SeparationRadius };
        PhysicsShapeQueryParameters2D query = new PhysicsShapeQueryParameters2D
        {
            Shape = searchShape,
            Transform = new Transform2D(0f, combatant.GlobalPosition),
            CollideWithAreas = false,
            CollideWithBodies = true,
            Exclude = new Array<Rid> { combatant.GetRid() }
        };

        Array<Dictionary> hits = combatant.GetWorld2D().DirectSpaceState.IntersectShape(query, 12);
        Vector2 repulsion = Vector2.Zero;

        foreach (Dictionary hit in hits)
        {
            if (!hit.ContainsKey("collider"))
            {
                continue;
            }

            if (hit["collider"].AsGodotObject() is not CollisionObject2D body)
            {
                continue;
            }

            if (ReferenceEquals(body, target))
            {
                continue;
            }

            Vector2 away = combatant.GlobalPosition - body.GlobalPosition;
            float distance = away.Length();
            if (distance <= 0.001f)
            {
                continue;
            }

            float clampedDistance = Mathf.Max(distance, MinSeparationDistance);
            float strength = 1f - Mathf.Clamp(clampedDistance / SeparationRadius, 0f, 1f);
            repulsion += away.Normalized() * strength;
        }

        if (repulsion == Vector2.Zero)
        {
            return Vector2.Zero;
        }

        return repulsion.Normalized();
    }

    private float GetPathClearance(Vector2 direction, float distance)
    {
        Vector2 from = combatant.GlobalPosition;
        Vector2 to = from + direction * distance;
        PhysicsRayQueryParameters2D rayQuery = PhysicsRayQueryParameters2D.Create(from, to);
        rayQuery.CollideWithAreas = false;
        rayQuery.CollideWithBodies = true;
        rayQuery.Exclude = new Array<Rid> { combatant.GetRid() };

        Dictionary hit = combatant.GetWorld2D().DirectSpaceState.IntersectRay(rayQuery);
        if (hit.Count == 0)
        {
            return distance;
        }

        if (hit.ContainsKey("collider") && hit["collider"].AsGodotObject() is GodotObject hitObject && ReferenceEquals(hitObject, target as GodotObject))
        {
            return distance;
        }

        if (!hit.ContainsKey("position"))
        {
            return 0f;
        }

        Vector2 hitPosition = hit["position"].AsVector2();
        return from.DistanceTo(hitPosition);
    }

    private void OnTargetDefeated()
    {
        combatant.ChangeState(new IdleState(combatant));
    }
}