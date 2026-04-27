using Godot;
using System;

public interface ITargetable
{
    public CoreAttributes CoreAttributes { get; }
    public DefensiveAttributes DefensiveAttributes { get; }
    public Vector2 Position { get; }
    public event Action Defeated;
    public void TakeDamage(float damage);
    public void Die();
    public int uid { get; }
}