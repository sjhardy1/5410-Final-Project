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
    public delegate void TimerChangedEventHandler(float downtimeTimeRemaining);

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
    public int pendingConstruction = 0;
    public int kitId = 0;
    public int difficulty = 0;
    public Dictionary<string, Variant> PurchasedUpgrades { get; private set; } = new Dictionary<string, Variant>();
    public Dictionary<string, AudioStream> audioCache = new Dictionary<string, AudioStream>();
    public AudioStreamPlayer musicPlayer = new AudioStreamPlayer();
    public AudioStreamPlayer sfxPlayer = new AudioStreamPlayer();
    public int masterVolume = 0;
    public int sfxVolume = 0;
    public int musicVolume = 0;

    public override void _Ready()
    {
        DowntimeTimeRemaining = defaultDowntimeSeconds;
        audioCache["relaxed"] = GD.Load<AudioStream>("res://Assets/Audio/chill_music_looped.tres");
        audioCache["intense"] = GD.Load<AudioStream>("res://Assets/Audio/intense_music_looped.tres");
        audioCache["arrow"] = GD.Load<AudioStream>("res://Assets/Audio/arrow.wav");
        audioCache["slash"] = GD.Load<AudioStream>("res://Assets/Audio/slash.wav");
        audioCache["magic"] = GD.Load<AudioStream>("res://Assets/Audio/magic.wav");
        audioCache["denied"] = GD.Load<AudioStream>("res://Assets/Audio/denied.wav");
        musicPlayer.Stream = audioCache["relaxed"];
        AddChild(musicPlayer);
        musicPlayer.Play();
        AddChild(sfxPlayer);
        RegisterVolume();
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        if (Phase == RunPhase.Downtime)
        {
            DowntimeTimeRemaining = Mathf.Max(0f, DowntimeTimeRemaining - dt);
            EmitSignal(nameof(TimerChanged), DowntimeTimeRemaining);

            if (DowntimeTimeRemaining <= 0f)
            {
                SetPhase(RunPhase.Raid);
                GetNode<SignalBus>("/root/SignalBus").PublishRaidBegin();
            }
        }
    }

    public void StartDowntime(float durationSeconds = -1f)
    {
        float duration = durationSeconds > 0f ? durationSeconds : defaultDowntimeSeconds;
        DowntimeTimeRemaining = duration;
        SetPhase(RunPhase.Downtime);
        EmitSignal(nameof(TimerChanged), DowntimeTimeRemaining);
    }
    public void UpdateVolume(int master, int music, int sfx)
    {
        masterVolume = master;
        musicVolume = music;
        sfxVolume = sfx;
        RegisterVolume();
    }
    private void RegisterVolume(){
        musicPlayer.VolumeDb = -10f + masterVolume + musicVolume;
        sfxPlayer.VolumeDb = -10f + masterVolume + sfxVolume;
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
            if (Phase == RunPhase.Paused && ActiveCombatants.Count > 0)
            {
                SetPhase(RunPhase.Raid);
            }
            else
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
        Food = 100 + (GetUpgradeLevel("extra_food") * 20);
        Wood = 40 + (GetUpgradeLevel("extra_wood") * 10);
        Wave = 1;
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
        EmitSignal(nameof(TimerChanged), DowntimeTimeRemaining);
    }

    public Dictionary<string, Variant> ToSaveData(GridPlaceable activePlaceable = null)
    {
        return new Dictionary<string, Variant>
        {
            { "current_round", Wave },
            { "difficulty", difficulty},
            { "wave", Wave },
            { "food", Food },
            { "wood", Wood },
            { "downtime_remaining", DowntimeTimeRemaining },
            { "pending_construction", pendingConstruction },
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
        SetPhase(RunPhase.Downtime);

        pendingConstruction = data.ContainsKey("pending_construction") ? (int)data["pending_construction"] : 0;
        Food = data.ContainsKey("food") ? (int)data["food"] : 200;
        Wood = data.ContainsKey("wood") ? (int)data["wood"] : 100;
        Wave = data.ContainsKey("current_round") ? (int)data["current_round"] : data.ContainsKey("wave") ? (int)data["wave"] : 1;
        difficulty = data.ContainsKey("difficulty") ? (int)data["difficulty"] : 0;
        GD.Print($"Loaded run with difficulty {difficulty} and wave {Wave}");
        DowntimeTimeRemaining = data.ContainsKey("downtime_remaining") ? (float)data["downtime_remaining"] : defaultDowntimeSeconds;
        LoadedActivePlaceablesData = ReadPlaceableArray(data, "active_placeables");
        LoadedStoredPlaceablesData = ReadPlaceableArray(data, "stored_placeables");

        EmitSignal(nameof(PhaseChanged), (int)Phase, (int)Phase);
        EmitSignal(nameof(ResourcesChanged), Food, Wood, MetaCurrency);
        EmitSignal(nameof(WaveChanged), Wave);
        EmitSignal(nameof(TimerChanged), DowntimeTimeRemaining);
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
        if(next == RunPhase.Raid)
        {
            musicPlayer.Stream = audioCache["intense"];
            musicPlayer.Play();
        } else if (next == RunPhase.Downtime)
        {
            musicPlayer.Stream = audioCache["relaxed"];
            musicPlayer.Play();
        }
        RunPhase previous = Phase;
        Phase = next;
        EmitSignal(nameof(PhaseChanged), (int)previous, (int)Phase);
    }

    public void LoadMetaState(Dictionary<string, Variant> metaData)
    {
        if (metaData == null)
        {
            PurchasedUpgrades = new Dictionary<string, Variant>();
            return;
        }
        if (metaData.ContainsKey("crystals"))
        {
            MetaCurrency = (int)metaData["crystals"];
        }
        else
        {
            MetaCurrency = 0;
        }
        if (metaData.ContainsKey("upgrades"))
        {
            PurchasedUpgrades = (Dictionary<string, Variant>)metaData["upgrades"];
        }
        else
        {
            PurchasedUpgrades = new Dictionary<string, Variant>();
        }
        if(metaData.ContainsKey("audio_settings"))
        {
            Dictionary<string, Variant> audioSettings = (Dictionary<string, Variant>)metaData["audio_settings"];
            masterVolume = audioSettings.ContainsKey("master") ? (int)audioSettings["master"] : 0;
            musicVolume = audioSettings.ContainsKey("music") ? (int)audioSettings["music"] : 0;
            sfxVolume = audioSettings.ContainsKey("sfx") ? (int)audioSettings["sfx"] : 0;
        }
        else
        {
            masterVolume = 0;
            musicVolume = 0;
            sfxVolume = 0;
        }
        RegisterVolume();
    }

    public Dictionary<string, Variant> ToMetaData()
    {
        return new Dictionary<string, Variant>
        {
            { "crystals", MetaCurrency },
            { "upgrades", PurchasedUpgrades },
            {"audio_settings", new Dictionary<string, Variant>{
                {"master", masterVolume},
                {"music", musicVolume},
                {"sfx", sfxVolume}
            } }
        };
    }

    public bool TryPurchaseUpgrade(string upgradeId)
    {
        if(GetUpgradeLevel(upgradeId) >= GetUpgradeMaxLevel(upgradeId)) return false;
        if(MetaCurrency < GetUpgradeCost(upgradeId, GetUpgradeLevel(upgradeId))) return false;
        MetaCurrency -= GetUpgradeCost(upgradeId, GetUpgradeLevel(upgradeId));
        if(!PurchasedUpgrades.ContainsKey(upgradeId))
        {
            PurchasedUpgrades[upgradeId] = 0;
        }
        PurchasedUpgrades[upgradeId] = (int)PurchasedUpgrades[upgradeId] + 1;
        return true;
    }

    public bool HasUpgrade(string upgradeId)
    {
        return PurchasedUpgrades.ContainsKey(upgradeId) && (int)PurchasedUpgrades[upgradeId] > 0;
    }

    public int GetUpgradeLevel(string upgradeId)
    {
        return PurchasedUpgrades.ContainsKey(upgradeId) ? (int)PurchasedUpgrades[upgradeId] : 0;
    }

    public int GetUpgradeMaxLevel(string upgradeId)
    {
        return upgradeId switch 
        {
            "extra_food" => 5,
            "extra_wood" => 5,
            "kit_1" => 1,
            "kit_2" => 1,
            "kit_3" => 1,
            _ => 0
        };
    }
    public int GetUpgradeCost(string upgradeId, int level)
    {
        return upgradeId switch 
        {
            "extra_food" => 5 * (level + 1),
            "extra_wood" => 5 * (level + 1),
            "kit_1" => 10,
            "kit_2" => 20,
            "kit_3" => 30,
            _ => int.MaxValue
        };
    }
    public string GetUpgradeDisplayName(string upgradeId)
    {
        return upgradeId switch 
        {
            "extra_food" => "Extra Starting Food",
            "extra_wood" => "Extra Starting Wood",
            "kit_1" => "Shepherd's Kit",
            "kit_2" => "Hunter's Kit",
            "kit_3" => "Giant's Kit",
            _ => "Unknown Upgrade"
        };
    }
}
