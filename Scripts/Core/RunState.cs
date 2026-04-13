using Godot;
using System;
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
    public Collections.List<PlaceableDefinition> ActivePlaceables { get; private set; } = new Collections.List<PlaceableDefinition>();
    public Collections.List<PlaceableDefinition> StoredPlaceables { get; private set; } = new Collections.List<PlaceableDefinition>();
    public int Wave { get; private set; } = 1;
    public float DowntimeTimeRemaining { get; private set; }
    public float RaidTimeElapsed { get; private set; }

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
            GetNode<SignalBus>("/root/SignalBus").PublishRaidEnded(Wave);
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
        Food = 100;
        Wood = 75;
        Wave = 1;
        RaidTimeElapsed = 0f;
        DowntimeTimeRemaining = defaultDowntimeSeconds;

        EmitSignal(nameof(PhaseChanged), (int)RunPhase.Downtime, (int)RunPhase.Downtime);
        EmitSignal(nameof(ResourcesChanged), Food, Wood, MetaCurrency);
        EmitSignal(nameof(WaveChanged), Wave);
        EmitSignal(nameof(TimerChanged), DowntimeTimeRemaining, RaidTimeElapsed);
    }

    public Dictionary<string, Variant> ToSaveData()
    {
        return new Dictionary<string, Variant>
        {
            { "phase", (int)Phase },
            { "food", Food },
            { "wood", Wood },
            { "meta_currency", MetaCurrency },
            { "wave", Wave },
            { "downtime_remaining", DowntimeTimeRemaining },
            { "raid_elapsed", RaidTimeElapsed }
        };
    }

    public void LoadFromSaveData(Dictionary<string, Variant> data)
    {
        if (data == null)
        {
            return;
        }

        Phase = data.ContainsKey("phase") ? (RunPhase)(int)data["phase"] : RunPhase.Downtime;
        Food = data.ContainsKey("food") ? (int)data["food"] : 100;
        Wood = data.ContainsKey("wood") ? (int)data["wood"] : 75;
        MetaCurrency = data.ContainsKey("meta_currency") ? (int)data["meta_currency"] : MetaCurrency;
        Wave = data.ContainsKey("wave") ? (int)data["wave"] : 1;
        DowntimeTimeRemaining = data.ContainsKey("downtime_remaining") ? (float)data["downtime_remaining"] : defaultDowntimeSeconds;
        RaidTimeElapsed = data.ContainsKey("raid_elapsed") ? (float)data["raid_elapsed"] : 0f;

        EmitSignal(nameof(PhaseChanged), (int)Phase, (int)Phase);
        EmitSignal(nameof(ResourcesChanged), Food, Wood, MetaCurrency);
        EmitSignal(nameof(WaveChanged), Wave);
        EmitSignal(nameof(TimerChanged), DowntimeTimeRemaining, RaidTimeElapsed);
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
