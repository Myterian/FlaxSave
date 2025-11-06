// © 2025 byteslider UG. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlaxEditor.Content.Settings;
using FlaxEngine;

namespace FlaxSave;

/// <summary>SaveManager class.</summary>
public class SaveManager : GamePlugin
{
    /// <summary>The active instance of a save manager</summary>
    public static SaveManager Instance => instance ??= PluginManager.GetPlugin<SaveManager>();

    /// <summary>Version of the plugin</summary>
    public static Version PluginVersion => new(1, 1, 74);

    /// <summary>The currently loaded in-memory data from a savegame</summary>
    public Dictionary<Guid, string> ActiveSaveData { get; private set; } = new();

    /// <summary>The currently loaded list of meta datas for savegames</summary>
    public List<SaveMeta> SaveMetas { get; private set; }

    /// <summary>The save settings instance that is in use by the save system</summary>
    public FlaxSaveSettings SaveSettings => saveSettings ??= GameSettings.Load<FlaxSaveSettings>() ?? new();

    private static SaveManager instance;

    private FlaxSaveSettings saveSettings;

    private object saveLock = new();

    private float nextAutoSave = float.MaxValue;

    private bool isActiveTaskRunning = false;


    /// <summary>Event for when saving has started</summary>
    public event Action<Dictionary<Guid, string>> OnSaving;

    /// <summary>Event for when saving has finished</summary>
    public event Action OnSaved;

    /// <summary>Event for when reading a savegame from disk is done. Used for setting actors and others in the scene to the saved state.</summary>
    public event Action<Dictionary<Guid, string>> OnLoaded;

    private Task workerTask = null;
    private Func<Task> pendingGameSave = null;
    private Func<Task> pendingGameLoad = null;
    private Func<Task> pendingSettingSave = null;
    private Func<Task> pendingSettingLoad = null;
    private Func<Task> pendingGameDelete = null;


    public override void Initialize()
    {
        base.Initialize();

        IOOpertation metasIO = new() { Path = SaveSettings.SavegameMetaFile };
        SaveMetas = FileIO.ReadFromDisk<List<SaveMeta>>(metasIO).GetAwaiter().GetResult();
        SaveMetas ??= new();


#if FLAX_EDITOR
        if (!SaveSettings.SkipLoadingSettingsInEditor)
            RequestSettingsLoad();
#else
        RequestSettingsLoad();
#endif

        SetAutoSaveActive(SaveSettings.AutoSave);
    }

    public override void Deinitialize()
    {
        SetAutoSaveActive(false);

        lock (saveLock)
        {
            pendingGameSave = null;
            pendingGameLoad = null;
            pendingSettingSave = null;
            pendingSettingLoad = null;
        }

        workerTask?.Wait();
        instance = null;
        saveSettings = null;
        base.Deinitialize();
    }

    /// <summary>Starts saving the game state to disk right away or at the next possible opening, if a save/load operation is already happening</summary>
    /// <param name="savegameName">The display name of the savegame, as it should appear in-game. The file name will be generated.</param>
    /// <param name="customMetaData">Container for game-specific meta data</param>
    public void RequestGameSave(string savegameName = null, object customMetaData = null)
    {
        lock (saveLock)
        {
            pendingGameSave = () => SaveGameToDisk(savegameName, customMetaData);
            StartTaskQueue();
        }
    }

    /// <summary>Start saving the settings as defined in SaveSettings to disk right away or at the next possible opening, if a save/load operation is already happening</summary>
    public void RequestSettingsSave()
    {
        lock (saveLock)
        {
            pendingSettingSave = SaveSettingsToDisk;
            StartTaskQueue();
        }
    }

    /// <summary>Starts loading the game state from disk right away or at the next possible opening, if a save/load operation is already happening</summary>
    /// <param name="saveName">The name of the savegame (not the display name)</param>
    public void RequestGameLoad(string saveName)
    {
        lock (saveLock)
        {
            pendingGameLoad = () => LoadGameFromDisk(saveName);
            StartTaskQueue();
        }
    }

    /// <summary>Starts loading the settings from disk right away or at the next possible opening, if a save/load operation is already happening</summary>
    public void RequestSettingsLoad()
    {
        lock (saveLock)
        {
            pendingSettingLoad = LoadSettingsFromDisk;
            StartTaskQueue();
        }
    }

    /// <summary>Deletes a savegame. You cannot undo this.</summary>
    /// <param name="saveName">The name of the savegame (not the display name)</param>
    public void RequestGameDelete(string saveName)
    {
        lock (saveLock)
        {
            pendingGameDelete = () => DeleteFileFromDisk(saveName);
            StartTaskQueue();
        }
    }

    /// <summary>Sets savegame data for a component. This is useful, when a save component gets disabled and we want to update its data for the next time the game gets saved.</summary>
    /// <param name="id">The id of the component</param>
    /// <param name="content">The data to save</param>
    public void SetSaveData(Guid id, string content)
    {
        lock (saveLock)
        {
            ActiveSaveData[id] = content;
        }
    }

    /// <summary>Removes a specific data set from the currently active save data. Data will be removed from savegame with the next disk save.</summary>
    /// <param name="id">The id of the save data</param>
    public void RemoveSaveData(Guid id)
    {
        lock (saveLock)
        {
            ActiveSaveData.Remove(id);
        }
    }

    /// <summary>Removes all data from the ActiveSaveData. This can help with 'New Game' or 'New Game+', where you start a fresh playthrough without closing the application.</summary>
    public void ClearSaveData()
    {
        lock (saveLock)
        {
            ActiveSaveData.Clear();
        }
    }

    /// <summary>Opens the save directory in the file brower</summary>
    public void OpenSaveDirectory()
    {
        SaveSettings.OpenDirectory();
    }

    /// <summary>Enables and disables auto save. Auto saving is enabled on game initilization, if the auto save is enabled in the save settings.</summary>
    /// <param name="isActive">true for enable, false for disable</param>
    public void SetAutoSaveActive(bool isActive)
    {
        Scripting.LateUpdate -= AutoSave;
        if (!isActive) return;

        Scripting.LateUpdate += AutoSave;
        nextAutoSave = Time.GameTime + SaveSettings.AutoSaveIntervalSeconds;
    }

    /// <summary>Creates savegames in intervals</summary>
    private void AutoSave()
    {
        if (Time.GameTime < nextAutoSave)
            return;

        nextAutoSave = Time.GameTime + SaveSettings.AutoSaveIntervalSeconds;
        RequestGameSave();
    }


    /// <summary>Writes a savegame to disk. Invokes OnSaving before starting and OnSaved after finishing the write operation.</summary>
    /// <param name="savegameName">The display name of the save. Defaults to 'Auto-Save'.</param>
    /// <param name="customMetaData">Container for custom meta data</param>
    /// <returns>Task</returns>
    private async Task SaveGameToDisk(string savegameName = null, object customMetaData = null)
    {
        // Nothing to save
        if (OnSaving == null)
        {
            if (OnSaved != null)
                Scripting.InvokeOnUpdate(OnSaved);

            return;
        }

        foreach (Action<Dictionary<Guid, string>> handler in OnSaving.GetInvocationList())
        {
            try { handler(ActiveSaveData); }
            catch (Exception ex) { Debug.LogException(ex); }
        }

        string saveName = Guid.NewGuid().ToString();

        // Update meta data
        SaveMeta newMeta = new()
        {
            DisplayName = savegameName,
            SaveName = saveName,
            SaveDate = DateTime.UtcNow,
            SaveVersion = SaveSettings.SavegameVersion,
            CustomData = customMetaData,
            IsAutoSave = savegameName == null
        };

        SaveMetas.Add(newMeta);
        
        // Write to disk
        IOOpertation saveIO = new() { Path = SaveSettings.GetSaveFilePath(saveName), Data = ActiveSaveData };
        Task saveWrite = FileIO.WriteToDisk(saveIO);

        IOOpertation metaIO = new() { Path = SaveSettings.SavegameMetaFile, Data = SaveMetas };
        Task metaWrite = FileIO.WriteToDisk(metaIO);

        await Task.WhenAll(saveWrite, metaWrite);

        if (OnSaved != null)
            Scripting.InvokeOnUpdate(OnSaved);
    }

    /// <summary>Writes engine and custom settings data as defined in the SaveSettings to disk</summary>
    /// <returns>Task</returns>
    private async Task SaveSettingsToDisk()
    {
        List<object> assetInstanceDatas = new();

        lock (saveLock)
        {
            for (int i = 0; i < SaveSettings.Assets.Count; i++)
            {
                SaveSettings.Assets[i].Instance?.SaveAction();
                assetInstanceDatas.Add(SaveSettings.Assets[i].Instance);
            }
        }

        IOOpertation saveIO = new() { Path = SaveSettings.SettingsFile, Data = assetInstanceDatas };
        await FileIO.WriteToDisk(saveIO);
    }

    /// <summary>Reads a savegame from disk and updates save components. Make sure to call this after scenes are deserialized.</summary>
    /// <param name="saveName">Name of the savegame file</param>
    /// <returns>Task</returns>
    private async Task LoadGameFromDisk(string saveName)
    {
        try
        {
            IOOpertation saveIO = new() { Path = SaveSettings.GetSaveFilePath(saveName) };
            Dictionary<Guid, string> readTask = await FileIO.ReadFromDisk<Dictionary<Guid, string>>(saveIO);

            Scripting.InvokeOnUpdate(() =>
            {
                ActiveSaveData = readTask;
                OnLoaded?.Invoke(readTask);
            });
        }
        catch (Exception ex) { Debug.LogException(ex); }
    }

    /// <summary>Reads settings from disk and applies them accordingly</summary>
    /// <returns>Task</returns>
    private async Task LoadSettingsFromDisk()
    {
        try
        {
            IOOpertation loadIO = new() { Path = SaveSettings.SettingsFile };
            List<object> assetInstanceDatas = await FileIO.ReadFromDisk<List<object>>(loadIO);

            Scripting.InvokeOnUpdate(() =>
            {
                for (int i = 0; i < assetInstanceDatas.Count; i++)
                {
                    if (SaveSettings.Assets[i] == null || assetInstanceDatas[i] == null)
                        continue;

                    SaveSettings.Assets[i].Asset.WaitForLoaded();
                    SaveSettings.Assets[i].Asset.SetInstance(assetInstanceDatas[i]);

                    if (SaveSettings.Assets[i].Instance is ISavableAsset savable)
                        savable.LoadAction();
                }
            });
        }
        catch (Exception ex) { Debug.LogException(ex); }
    }

    /// <summary>
    /// Deletes a savegame from disk and removes its entry from the saves meta list
    /// </summary>
    /// <param name="saveName"></param>
    private async Task DeleteFileFromDisk(string saveName)
    {
        try
        {
            IOOpertation saveIO = new() { Path = SaveSettings.GetSaveFilePath(saveName) };
            await Task.Run(() => FileIO.DeleteFromDisk(saveIO));

            int index = SaveMetas.FindIndex(x => x.SaveName == saveName);

            if (-1 < index)
                SaveMetas.RemoveAt(index);
        }
        catch (Exception ex) { Debug.LogException(ex); }
    }

    /// <summary>Starts tasks based on priority</summary>
    private void StartTaskQueue()
    {
        lock (saveLock)
        {
            if (isActiveTaskRunning) return;
            isActiveTaskRunning = true;

            workerTask = Task.Run(async () =>
            {
                while (true)
                {
                    Func<Task> taskToRun = null;

                    lock (saveLock)
                    {
                        taskToRun = Take(ref pendingSettingSave)
                                    ?? Take(ref pendingSettingLoad)
                                    ?? Take(ref pendingGameSave)
                                    ?? Take(ref pendingGameLoad)
                                    ?? Take(ref pendingGameDelete);

                        if (taskToRun == null)
                        {
                            isActiveTaskRunning = false;
                            workerTask = null;
                            break;
                        }
                    }

                    try { await taskToRun(); }
                    catch (Exception ex) { Debug.LogException(ex); }
                }
            });
        }
    }

    /// <summary>Takes a task if available and resets it</summary>
    /// <param name="pending">The pending Func-Task</param>
    /// <returns>Func with Task, null nothing there</returns>
    private Func<Task> Take(ref Func<Task> pending)
    {
        if (pending == null)
            return null;

        var task = pending;
        pending = null;
        return task;
    }

    public SaveManager()
    {
        _description = new()
        {
            Name = "FlaxSave Runtime",
            Description = "A ready-to-use, component based save system for the Flax Engine",
            Category = "FlaxSave",
            Author = "Thomas Jungclaus",
            AuthorUrl = "https://github.com/Myterian/",
            IsAlpha = false,
            IsBeta = false,
            Version = PluginVersion
        };
    }


}
