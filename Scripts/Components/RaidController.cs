using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;
public partial class RaidController : Node2D
{
    private RunState runState;
    private SignalBus signalBus;
    [Export] public float spawnDelay = 1.0f;
    private float spawnTimer = 0f;
    private Queue<Combatant> waveQueue;
    private int nextUid = 1;
    public override void _Ready()
    {
        runState = GetNodeOrNull<RunState>("/root/RunState");
        signalBus = GetNodeOrNull<SignalBus>("/root/SignalBus");
        signalBus.UnitDied += RemoveUnit;
    }
    public override void _Process(double delta)
    {
        if (runState.Phase != RunPhase.Raid) return;
        // Spawn logic
        spawnTimer += (float)delta;
        if (spawnTimer >= spawnDelay)
        {
            spawnTimer -= spawnDelay;
            if (waveQueue != null && waveQueue.Count > 0)
            {
                GD.Print("Spawning enemy, " + waveQueue.Count + " remaining in queue.");
                Combatant nextEnemy = waveQueue.Dequeue();
                nextEnemy.uid = nextUid++;
                runState.ActiveCombatants.Add(nextEnemy);
                AddChild(nextEnemy);
            }
        }
        // Check for end of raid
        if (IsRaidOver())
        {
            signalBus.PublishRaidEnded();
            CleanupRaid();
        }
    }
    public void StartRaid(GameDatabase database)
    {
        waveQueue = GenerateWave(database, runState.Wave);
    }
    public bool IsRaidOver()
    {
        if(waveQueue == null || waveQueue.Count > 0) return false;
        foreach(Combatant combatant in runState.ActiveCombatants)
        {
            if(combatant.faction == Faction.Enemy) return false;
        }
        return true;
    }
    public void CleanupRaid()
    {
        waveQueue = null;
        foreach(Node child in GetChildren())
        {
            child.QueueFree();
        }
        runState.ActiveCombatants.Clear();
    }
    public Queue<Combatant> GenerateWave(GameDatabase database, int wave)
    {
        Queue<Combatant> queue = new Queue<Combatant>();
        int wavePower = wave * 25;
        Array<EnemyDefinition> enemies = database.GetAllEnemies();
        while (wavePower > 0)
        {
            Array<EnemyDefinition> affordableEnemies = new Array<EnemyDefinition>();
            foreach (EnemyDefinition enemy in enemies)
            {
                if (enemy.Value <= wavePower)
                {
                    affordableEnemies.Add(enemy);
                    break;
                }
            }
            if (affordableEnemies.Count == 0) break;
            EnemyDefinition chosenEnemy = affordableEnemies[GD.RandRange(0, affordableEnemies.Count - 1)];
            queue.Enqueue(new Combatant(chosenEnemy, Vector2.One.Rotated(GD.Randf() * Mathf.Pi * 2) * 500));
            wavePower -= chosenEnemy.Value;
        }
        return queue;
    }
    public void PlaceUnit(UnitDefinition def)
    {
        Combatant combatant = new Combatant(def);
        combatant.uid = nextUid++;
        runState.ActiveCombatants.Add(combatant);
        combatant.Position = def.AnchorCell * 64;
        AddChild(combatant);
    }

    public void RemoveUnit(int combatantUid)
    {
        Combatant toRemove = null;
        foreach(Combatant combatant in runState.ActiveCombatants)
        {
            if(combatant.uid == combatantUid)
            {
                toRemove = combatant;
                break;
            }
        }
        if(toRemove != null)
        {
            runState.ActiveCombatants.Remove(toRemove);
        }
    }
}
