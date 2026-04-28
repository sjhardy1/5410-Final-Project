using Godot;
using System;
using Godot.Collections;
public partial class SaveManager : Node
{
    [Signal]
    public delegate void SaveCompletedEventHandler(string savePath);

    [Signal]
    public delegate void LoadCompletedEventHandler(string savePath);

    [Signal]
    public delegate void SaveFailedEventHandler(string savePath, string reason);

    [Signal]
    public delegate void LoadFailedEventHandler(string savePath, string reason);

    [Export]
    public string saveDirectory = "user://saves";

    [Export]
    public string runSaveFileName = "run_state.save";

    [Export]
    public string metaSaveFileName = "meta_state.save";

    private const int SaveVersion = 1;

    public string GetRunSavePath()
    {
        return saveDirectory.PathJoin(runSaveFileName);
    }

    public string GetMetaSavePath()
    {
        return saveDirectory.PathJoin(metaSaveFileName);
    }

    public bool SaveRunState(RunState runState, GridPlaceable activePlaceable = null)
    {
        if (runState == null)
        {
            EmitSignal(nameof(SaveFailed), GetRunSavePath(), "RunState was null.");
            return false;
        }

        Dictionary<string, Variant> root = new Dictionary<string, Variant>
        {
            { "version", SaveVersion },
            { "saved_at_unix", Time.GetUnixTimeFromSystem() },
            { "run_state", (Variant)runState.ToSaveData(activePlaceable) }
        };

        bool ok = WriteDictionaryToPath(GetRunSavePath(), root, out string reason);
        if (!ok)
        {
            EmitSignal(nameof(SaveFailed), GetRunSavePath(), reason);
            return false;
        }

        EmitSignal(nameof(SaveCompleted), GetRunSavePath());
        return true;
    }

    public bool LoadRunState(RunState runState)
    {
        if (runState == null)
        {
            EmitSignal(nameof(LoadFailed), GetRunSavePath(), "RunState was null.");
            return false;
        }

        if (!FileAccess.FileExists(GetRunSavePath()))
        {
            EmitSignal(nameof(LoadFailed), GetRunSavePath(), "Save file does not exist.");
            return false;
        }

        bool ok = ReadDictionaryFromPath(GetRunSavePath(), out Dictionary<string, Variant> root, out string reason);
        if (!ok)
        {
            EmitSignal(nameof(LoadFailed), GetRunSavePath(), reason);
            return false;
        }

        if (!root.ContainsKey("run_state"))
        {
            EmitSignal(nameof(LoadFailed), GetRunSavePath(), "Missing run_state payload.");
            return false;
        }

        runState.LoadFromSaveData((Dictionary<string, Variant>)root["run_state"]);
        EmitSignal(nameof(LoadCompleted), GetRunSavePath());
        return true;
    }

    public bool SaveMetaState(Dictionary<string, Variant> metaState)
    {
        Dictionary<string, Variant> root = new Dictionary<string, Variant>
        {
            { "version", SaveVersion },
            { "saved_at_unix", Time.GetUnixTimeFromSystem() },
            { "meta_state", metaState ?? new Dictionary<string, Variant>() }
        };

        bool ok = WriteDictionaryToPath(GetMetaSavePath(), root, out string reason);
        if (!ok)
        {
            EmitSignal(nameof(SaveFailed), GetMetaSavePath(), reason);
            return false;
        }
        GD.Print("Meta state saved successfully.");
        EmitSignal(nameof(SaveCompleted), GetMetaSavePath());
        return true;
    }

    public Dictionary<string, Variant> LoadMetaState()
    {
        if (!FileAccess.FileExists(GetMetaSavePath()))
        {
            EmitSignal(nameof(LoadFailed), GetMetaSavePath(), "Meta save file does not exist.");
            return new Dictionary<string, Variant>();
        }

        bool ok = ReadDictionaryFromPath(GetMetaSavePath(), out Dictionary<string, Variant> root, out string reason);
        if (!ok)
        {
            EmitSignal(nameof(LoadFailed), GetMetaSavePath(), reason);
            return new Dictionary<string, Variant>();
        }

        if (!root.ContainsKey("meta_state"))
        {
            EmitSignal(nameof(LoadFailed), GetMetaSavePath(), "Missing meta_state payload.");
            return new Dictionary<string, Variant>();
        }

        EmitSignal(nameof(LoadCompleted), GetMetaSavePath());
        return (Dictionary<string, Variant>)root["meta_state"];
    }

    public bool HasRunSave()
    {
        return FileAccess.FileExists(GetRunSavePath());
    }

    public void DeleteRunSave()
    {
        string path = GetRunSavePath();
        if (FileAccess.FileExists(path))
        {
            DirAccess.RemoveAbsolute(path);
        }
    }

    private bool WriteDictionaryToPath(string path, Dictionary<string, Variant> data, out string reason)
    {
        reason = string.Empty;

        Error ensureDir = DirAccess.MakeDirRecursiveAbsolute(saveDirectory);
        if (ensureDir != Error.Ok && ensureDir != Error.AlreadyExists)
        {
            reason = "Failed creating save directory: " + ensureDir;
            return false;
        }

        using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (file == null)
        {
            reason = "Could not open file for writing.";
            return false;
        }

        string json = Json.Stringify(data);
        file.StoreString(json);
        return true;
    }

    private bool ReadDictionaryFromPath(string path, out Dictionary<string, Variant> result, out string reason)
    {
        result = new Dictionary<string, Variant>();
        reason = string.Empty;

        using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            reason = "Could not open file for reading.";
            return false;
        }

        string text = file.GetAsText();
        Variant parsed = Json.ParseString(text);

        if (parsed.VariantType != Variant.Type.Dictionary)
        {
            reason = "Save file was not a dictionary JSON object.";
            return false;
        }

        result = (Dictionary<string, Variant>)parsed;
        return true;
    }
}
