using Godot;
using Collections = System.Collections.Generic;
using Godot.Collections;

public enum RunPhase
{
    Downtime,
    Raid,
    Paused,
    Victory,
    Defeat
}

public partial class RunState : Node
{
    [Signal]
    public delegate void PhaseChangedEventHandler(RunPhase previous, RunPhase current);

    [Signal]
    public delegate void ResourcesChangedEventHandler(int food, int wood, int metaCurrency);

    [Signal]
    public delegate void WaveChangedEventHandler(int wave);

    [Signal]
    public delegate void TimerChangedEventHandler(float downtimeTimeRemaining, float raidTimeElapsed);

    [Export]
    public float defaultDowntimeSeconds = 180f;

    public RunPhase Phase { get; private set; } = RunPhase.Downtime;
    public int Food { get; private set; } = 200;
    public int Wood { get; private set; } = 100;
    public int MetaCurrency { get; private set; } = 0;
    public Collections.List<GridPlaceable> ActivePlaceables { get; private set; } = new Collections.List<GridPlaceable>();
    public Collections.List<GridPlaceable> StoredPlaceables { get; private set; } = new Collections.List<GridPlaceable>();
    public Collections.List<Combatant> ActiveCombatants { get; private set; } = new Collections.List<Combatant>();
    public Collections.List<CombatObject> ActiveObjects { get; private set; } = new Collections.List<CombatObject>();
    public Array LoadedActivePlaceablesData { get; private set; } = new Array();
    public Array LoadedStoredPlaceablesData { get; private set; } = new Array();
    public int Wave { get; private set; } = 1;
    public float DowntimeTimeRemaining { get; private set; }
    public float RaidTimeElapsed { get; private set; }
    public int pendingConstruction = 0;

    public SignalBus signalBus;

    public override void _Ready()
    {
        DowntimeTimeRemaining = defaultDowntimeSeconds;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        if (Phase == RunPhase.Downtime)
        {
            DowntimeTimeRemaining = Mathf.Max(0f, DowntimeTimeRemaining - dt);
            EmitSignal(nameof(TimerChanged), DowntimeTimeRemaining, RaidTimeElapsed);

            if (DowntimeTimeRemaining <= 0f)
            {
                SetPhase(RunPhase.Raid);
            }
        }
        else if (Phase == RunPhase.Raid)
        {
            RaidTimeElapsed += dt;
            EmitSignal(nameof(TimerChanged), DowntimeTimeRemaining, RaidTimeElapsed);
        }
    }

    public void StartDowntime(float durationSeconds = -1f)
    {
        float duration = durationSeconds > 0f ? durationSeconds : defaultDowntimeSeconds;
        DowntimeTimeRemaining = duration;
        RaidTimeElapsed = 0f;
        SetPhase(RunPhase.Downtime);
        EmitSignal(nameof(TimerChanged), DowntimeTimeRemaining, RaidTimeElapsed);
    }

    public void StartRaid()
    {
        SetPhase(RunPhase.Raid);
    }

    public void SetPaused(bool paused)
    {
        if (paused)
        {
            if (Phase == RunPhase.Downtime || Phase == RunPhase.Raid)
            {
                SetPhase(RunPhase.Paused);
            }
        }
        else
        {
            if (Phase == RunPhase.Paused)
            {
                SetPhase(RunPhase.Downtime);
            }
        }
    }

    public void AddResources(int foodDelta, int woodDelta)
    {
        Food = Mathf.Max(0, Food + foodDelta);
        Wood = Mathf.Max(0, Wood + woodDelta);
        EmitSignal(nameof(ResourcesChanged), Food, Wood, MetaCurrency);
    }

    public bool TrySpendResources(int foodCost, int woodCost)
    {
        if (Food < foodCost || Wood < woodCost)
        {
            return false;
        }

        Food -= foodCost;
        Wood -= woodCost;
        EmitSignal(nameof(ResourcesChanged), Food, Wood, MetaCurrency);
        return true;
    }

    public void ForceSpendResources(int foodCost, int woodCost)
    {
        Food = Mathf.Max(0, Food - foodCost);
        Wood = Mathf.Max(0, Wood - woodCost);
        EmitSignal(nameof(ResourcesChanged), Food, Wood, MetaCurrency);
    }

    public void AddMetaCurrency(int amount)
    {
        MetaCurrency = Mathf.Max(0, MetaCurrency + amount);
        EmitSignal(nameof(ResourcesChanged), Food, Wood, MetaCurrency);
    }

    public void AdvanceWave()
    {
        Wave += 1;
        EmitSignal(nameof(WaveChanged), Wave);
    }

    public void MarkVictory()
    {
        SetPhase(RunPhase.Victory);
    }

    public void MarkDefeat()
    {
        SetPhase(RunPhase.Defeat);
    }

    public void ResetRun()
    {
        Phase = RunPhase.Downtime;
        Food = 200;
        Wood = 100;
        MetaCurrency = 0;
        Wave = 1;
        RaidTimeElapsed = 0f;
        DowntimeTimeRemaining = defaultDowntimeSeconds;
        pendingConstruction = 0;
        ActivePlaceables.Clear();
        StoredPlaceables.Clear();
        ActiveCombatants.Clear();
        ActiveObjects.Clear();
        LoadedActivePlaceablesData = new Array();
        LoadedStoredPlaceablesData = new Array();

        EmitSignal(nameof(PhaseChanged), (int)RunPhase.Downtime, (int)RunPhase.Downtime);
        EmitSignal(nameof(ResourcesChanged), Food, Wood, MetaCurrency);
        EmitSignal(nameof(WaveChanged), Wave);
        EmitSignal(nameof(TimerChanged), DowntimeTimeRemaining, RaidTimeElapsed);
    }

    public Dictionary<string, Variant> ToSaveData(GridPlaceable activePlaceable = null)
    {
        return new Dictionary<string, Variant>
        {
            { "phase", (int)Phase },
            { "current_round", Wave },
            { "wave", Wave },
            { "food", Food },
            { "wood", Wood },
            { "meta_currency", MetaCurrency },
            { "downtime_remaining", DowntimeTimeRemaining },
            { "raid_elapsed", RaidTimeElapsed },
            { "active_placeables", SerializePlaceables(ActivePlaceables, true) },
            { "stored_placeables", SerializePlaceables(StoredPlaceables, false, activePlaceable) }
        };
    }

    public void LoadFromSaveData(Dictionary<string, Variant> data)
    {
        if (data == null)
        {
            ResetRun();
            return;
        }

        ActivePlaceables.Clear();
        StoredPlaceables.Clear();
        ActiveCombatants.Clear();
        ActiveObjects.Clear();
        pendingConstruction = 0;

        Phase = data.ContainsKey("phase") ? (RunPhase)(int)data["phase"] : RunPhase.Downtime;
        Food = data.ContainsKey("food") ? (int)data["food"] : 200;
        Wood = data.ContainsKey("wood") ? (int)data["wood"] : 100;
        MetaCurrency = data.ContainsKey("meta_currency") ? (int)data["meta_currency"] : 0;
        Wave = data.ContainsKey("current_round") ? (int)data["current_round"] : data.ContainsKey("wave") ? (int)data["wave"] : 1;
        DowntimeTimeRemaining = data.ContainsKey("downtime_remaining") ? (float)data["downtime_remaining"] : defaultDowntimeSeconds;
        RaidTimeElapsed = data.ContainsKey("raid_elapsed") ? (float)data["raid_elapsed"] : 0f;
        LoadedActivePlaceablesData = ReadPlaceableArray(data, "active_placeables");
        LoadedStoredPlaceablesData = ReadPlaceableArray(data, "stored_placeables");

        EmitSignal(nameof(PhaseChanged), (int)Phase, (int)Phase);
        EmitSignal(nameof(ResourcesChanged), Food, Wood, MetaCurrency);
        EmitSignal(nameof(WaveChanged), Wave);
        EmitSignal(nameof(TimerChanged), DowntimeTimeRemaining, RaidTimeElapsed);
    }

    private static Array SerializePlaceables(Collections.List<GridPlaceable> placeables, bool includeAnchorCell, GridPlaceable extraPlaceable = null)
    {
        Array serializedPlaceables = new Array();
        foreach (GridPlaceable placeable in placeables)
        {
            if (placeable?.def?.CoreAttributes == null)
            {
                continue;
            }

            Dictionary<string, Variant> placeableData = new Dictionary<string, Variant>
            {
                { "id", placeable.def.CoreAttributes.Id }
            };

            if (includeAnchorCell)
            {
                placeableData["anchor"] = new Dictionary<string, Variant>
                {
                    { "x", placeable.AnchorCell.X },
                    { "y", placeable.AnchorCell.Y }
                };
            }

            serializedPlaceables.Add(placeableData);
        }

        if (extraPlaceable?.def?.CoreAttributes != null)
        {
            serializedPlaceables.Add(new Dictionary<string, Variant>
            {
                { "id", extraPlaceable.def.CoreAttributes.Id }
            });
        }

        return serializedPlaceables;
    }

    private static Array ReadPlaceableArray(Dictionary<string, Variant> data, string key)
    {
        if (!data.ContainsKey(key))
        {
            return new Array();
        }

        Variant value = data[key];
        if (value.VariantType != Variant.Type.Array)
        {
            return new Array();
        }

        return (Array)value;
    }

    private void SetPhase(RunPhase next)
    {
        if (Phase == next)
        {
            return;
        }

        RunPhase previous = Phase;
        Phase = next;
        EmitSignal(nameof(PhaseChanged), (int)previous, (int)Phase);
    }
}
