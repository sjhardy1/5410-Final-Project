using Godot;

public static class CombatRangeUtility
{
    public static bool IsTargetInAttackRange(Combatant combatant, ITargetable target)
    {
        float attackRangePixels = combatant.OffensiveAttributes.AttackRange * 64f;
        float distanceToHitbox = GetDistanceToTargetHitbox(combatant.GlobalPosition, target);
        return distanceToHitbox <= attackRangePixels;
    }

    private static float GetDistanceToTargetHitbox(Vector2 sourcePosition, ITargetable target)
    {
        if (target is not CollisionObject2D collisionTarget)
        {
            return sourcePosition.DistanceTo(target.Position);
        }

        float closestDistance = float.MaxValue;
        bool foundShape = false;

        foreach (Node child in collisionTarget.GetChildren())
        {
            if (child is not CollisionShape2D shapeNode || shapeNode.Disabled || shapeNode.Shape == null)
            {
                continue;
            }

            foundShape = true;
            float shapeDistance = DistanceToShapeBoundary(shapeNode, sourcePosition);
            if (shapeDistance < closestDistance)
            {
                closestDistance = shapeDistance;
            }
        }

        if (foundShape)
        {
            return closestDistance;
        }

        return sourcePosition.DistanceTo(target.Position);
    }

    private static float DistanceToShapeBoundary(CollisionShape2D shapeNode, Vector2 point)
    {
        if (shapeNode.Shape is CircleShape2D circle)
        {
            return Mathf.Max(0f, shapeNode.GlobalPosition.DistanceTo(point) - circle.Radius);
        } else if (shapeNode.Shape is RectangleShape2D rect)
        {
            Vector2 localPoint = shapeNode.ToLocal(point);
            Vector2 halfExtents = rect.Size * 0.5f;
            Vector2 clampedPoint = new Vector2(
                Mathf.Clamp(localPoint.X, -halfExtents.X, halfExtents.X),
                Mathf.Clamp(localPoint.Y, -halfExtents.Y, halfExtents.Y)
            );
            Vector2 closestPoint = shapeNode.ToGlobal(clampedPoint);
            return closestPoint.DistanceTo(point);
        }

        return shapeNode.GlobalPosition.DistanceTo(point);
    }
}
