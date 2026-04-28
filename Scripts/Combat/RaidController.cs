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
    private int repairCost = 0;
    private int healCost = 0;
    private List<ITargetable> targetablesToRemove = new List<ITargetable>();
    public override void _Ready()
    {
        runState = GetNodeOrNull<RunState>("/root/RunState");
        signalBus = GetNodeOrNull<SignalBus>("/root/SignalBus");
        signalBus.UnitDied += RemoveUnit;
        signalBus.BuildingDestroyed += RemoveBuilding;
    }
    public override void _PhysicsProcess(double delta)
    {
        if (runState.Phase != RunPhase.Raid){
            foreach(Combatant combatant in runState.ActiveCombatants)
            {
                combatant.LinearVelocity = Vector2.Zero;
            }
            return;
        }
        foreach(Combatant combatant in runState.ActiveCombatants)
        {
            combatant.Process(delta);
        }
        foreach(ITargetable targetable in targetablesToRemove)
        {
            if(targetable is Combatant combatant)
            {
                runState.ActiveCombatants.Remove(combatant);

            }
            else if(targetable is CombatObject building)
            {
                runState.ActiveObjects.Remove(building);
            }
        }
        targetablesToRemove.Clear();
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
            signalBus.PublishRaidEnded(healCost, repairCost);
            CleanupRaid();
        }
        if (isGameLost())
        {
            signalBus.PublishGameLost();
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
    private bool isGameLost()
    {
        if(waveQueue == null) return false;
        foreach(CombatObject building in runState.ActiveObjects)
        {
            if(building.CoreAttributes.Id == "town_center") return false;
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
        runState.ActiveObjects.Clear();
    }
    public Queue<Combatant> GenerateWave(GameDatabase database, int wave)
    {
        Queue<Combatant> queue = new Queue<Combatant>();
        int wavePower = wave *  20 + runState.difficulty * 5;
        Array<EnemyDefinition> enemies = database.GetAllEnemies();
        while (wavePower > 0)
        {
            Array<EnemyDefinition> affordableEnemies = new Array<EnemyDefinition>();
            foreach (EnemyDefinition enemy in enemies)
            {
                if (enemy.Value <= wavePower && enemy.minimumWave <= wave)
                {
                    affordableEnemies.Add(enemy);
                }
            }
            if (affordableEnemies.Count == 0) break;
            EnemyDefinition chosenEnemy = affordableEnemies[GD.RandRange(0, affordableEnemies.Count - 1)];
            queue.Enqueue(new Combatant(chosenEnemy, Vector2.One.Rotated(GD.Randf() * Mathf.Pi * 2) * 500));
            wavePower -= chosenEnemy.Value;
        }
        return queue;
    }
    public void PlaceUnit(GridPlaceable placeable)
    {
        Combatant combatant = new Combatant(placeable.def as UnitDefinition);
        combatant.uid = nextUid++;
        foreach(GridPlaceable buffer in runState.ActivePlaceables)
        {
            if(buffer.def.CoreAttributes.Id == "sheep_farm")
            {
                combatant.DefensiveAttributes.MaxHealth += 50;
                combatant.DefensiveAttributes.Health += 50;
            }
            if(buffer.def.CoreAttributes.Id == "barracks")
            {
                combatant.OffensiveAttributes.AttackDamage += 10;
                combatant.moveSpeed += 25;
            }
            if(buffer.def.CoreAttributes.Id == "workshop")
            {
                combatant.OffensiveAttributes.AttackCooldown = Mathf.Pow(combatant.OffensiveAttributes.AttackCooldown * 5, 0.8f) / 5;
            }
        }
        runState.ActiveCombatants.Add(combatant);
        combatant.Position = placeable.AnchorCell * 64 + Vector2.One * 32;
        AddChild(combatant);
    }
    public void PlaceBuilding(GridPlaceable placeable)
    {
        CombatObject building = new CombatObject(placeable.def as BuildingDefinition);
        building.uid = nextUid++;
        building.Position = placeable.AnchorCell * 64;
        runState.ActiveObjects.Add(building);
        AddChild(building);   
    }

    public void RemoveUnit(int combatantUid)
    {
        Combatant toRemove = null;
        foreach(Combatant combatant in runState.ActiveCombatants)
        {
            if(combatant.uid == combatantUid)
            {
                if(combatant.faction == Faction.Ally)
                {
                    healCost += 10;
                }
                toRemove = combatant;
                break;
            }
        }
        if(toRemove != null)
        {
            targetablesToRemove.Add(toRemove);
        }
    }
    public void RemoveBuilding(int buildingUid)
    {
        CombatObject toRemove = null;
        foreach(CombatObject building in runState.ActiveObjects)
        {
            if(building.uid == buildingUid)
            {
                repairCost += 20;
                toRemove = building;
                break;
            }
        }
        if(toRemove != null)
        {
            targetablesToRemove.Add(toRemove);
        }
    }
}
