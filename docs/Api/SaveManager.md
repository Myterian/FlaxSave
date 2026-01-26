---
title: SaveManager
---

# Class SaveManager
The `SaveManager` is the central hub for saving and loading game and asset states. The class uses a singleton pattern, to provide easy, global accessibility for active savegames and everything necessary around it.

## Properties

### [Instance](SaveManager.md#instance)
Allows for easy access to the singleton instance of the [SaveManager](SaveManager.md). Returns null, if the plugin wasn't loaded, yet.

- Type: `FlaxSave.SaveManager`
- Access: `static`

``` cs title="C#"
var manager = SaveManager.Instance;

```
</br>

### [PluginVersion](SaveManager.md#pluginversion)
Gets the current version of the plugin. This property can be used to verify engine compatibility or to check for available features.  

- Type: `System.Version`
- Access: `static`

!!! info "`PluginVersion` vs. `SavegameVersion`"
    The `PluginVersion` property tracks the software version of the plugin itself. For tracking data schema changes and handling migrations of individual save files, use the versioning system via [SavegameVersion](FlaxSaveSettings.md) within the [SaveMeta](SaveMeta.md#saveversion).
 
</br>

### [ActiveSaveData](SaveManager.md#activesavedata)
A dictionary that holds the loaded data of the active savegame. 

- Type: `Dictionary<Guid, string>`
- Key: The unique `Guid` of an object or asset
- Value: The persistent data as JSON-string
- Note: This property is `private set`. Changes should be done via [SetSaveData](#setsavedata) or during the [OnSaving](#onsaving) event.

</br>



### [SaveMetas](SaveManager.md#savemetas)
Provides a chronological list of all detected save games. This collection is primarily used to populate "Load Game" menus with metadata (i.e. timestamps, playtimes, or names).

- Type: `List<SaveMeta>`
- Sorting: The list is ordered chronologically, where the oldest save game is at index `0`.

!!! Tip
    To load a specific savegame, retrive its [SaveName](SaveMeta.md#savename) from the list and pass it to the loading system.

``` cs title="C#"
// Load the oldest savegame
SaveMeta oldestSave = SaveManager.Instance.SaveMetas[0];
string saveName = oldestSave.SaveName;
SaveManager.Instance.RequestGameLoad(saveName);

// Load the newest savegame
SaveMeta newestSave = SaveManager.Instance.SaveMetas[^1];
string saveName = oldestSave.SaveName;
SaveManager.Instance.RequestGameLoad(saveName);

```
</br>

### [SaveSettings](SaveManager.md#savesettings)
Provides access to the active save settings configuration. This property acts as the central hub for project-specific rules, such as save data versioning and file paths. The `SaveManager` retrieves these settings from the [FlaxSaveSettings](FlaxSaveSettings.md) asset within your project's `GameSettings` asset.

- Type: `FlaxSave.FlaxSaveSettings`

``` cs title="C#"
// Compare the current savegame version to the oldest savegame
Version settingsVersion = SaveManager.Instance.SaveSettings.SavegameVersion;
Version saveVersion = SaveManager.Instance.SaveMetas[0].SaveVersion;

if(saveVersion != settingsVersion)
    Debug.Log("The savegame seems to be older. Proceed with caution!");
```
</br>

---

## Events

### [OnSaving](SaveManager.md#onsaving)
This event is triggered immediatly after a save operation is initiated, but before any data is being written to disk. It acts as a data collection hook for all persistent objects within your project.

- Type: `Action<Dictionary<Guid, string>>`
- Payload: The active save data dictionary to be populated

!!! success "Performance: Non-Blocking" 
    This event does not hinder gameplay. You can serialize large amounts of data without causing frame drops or "hitching."

!!! info "Read, don't write"
    This event runs on a background thread, while the regular game keeps running on the main thread. Flax does not allow scene changes from a background thread.

    It is safe to read values from Actors and Scripts (i.e. `Actor.Position`, `Light.Brightness`, `Image.Visible`, etc), but avoid changing values in the scene or updating the UI (i.e. don't do `player.Health = 100` or `label.Text = "Saving..."`) from your listener method.

``` cs title="C#"
private void CollectMyData(Dictionary<Guid, string> saveData)
{
    // Only read data here
    Vector3 myPosition = Actor.Position;
    saveData[ID] = JsonSerializer.Serialize(myPosition);
}

public override void OnEnable()
{
    SaveManager.Instance.OnSaving += CollectMyData;
}

public override void OnDisable()
{
    // Don't forget to unsubscribe your methods from events!
    SaveManager.Instance.OnSaving -= CollectMyData;
}
```
</br>

### [OnSaved](SaveManager.md#onsaved)
This event is triggered after the background save operation has been completed and all the data has been written to disk.

- Type: `Action`

!!! tip "Main thread dispatch"
    Unlike [OnSaving](#onsaving), this event is dispatched back on the main thread. This allows you to safely call UI methods and interact with engine components without thead-safety concerns.

</br>

### [OnLoaded](SaveManager.md#onloaded)
This event is triggered after the background loading operation has been completed and all the data is available for further utilization.

- Type: `Action`

!!! tip "Main thread dispatch"
    Unlike [OnSaving](#onsaving), this event is dispatched back on the main thread. This allows you to safely set object data and interact with engine components without thead-safety concerns.

``` cs title="C#"
private void SpawnLevels()
{
    Level.LoadScene(GameSettings.Load().FirstScene);
}

public override void OnEnable()
{
    SaveManager.Instance.OnLoaded += SpawnLevels;
}

public override void OnDisable()
{
    // Don't forget to unsubscribe your methods from events!
    SaveManager.Instance.OnLoaded -= SpawnLevels;
}
```
</br>

---

## Methods

### [SetSaveData](SaveManager.md#setsavedata)
Provides thread-safe write access to the savegame data for a specific component. This method updates the in-memory save state without writing the save to disk.

|Parameter|Type|Description|
|----|---|---|
|id|Guid|Unique identifier of a save component|
|content|string|Save data associated with the component as a serialized JSON-string|

**How it works**: This method writes the provided serialized data to the [ActiveSaveData](#activesavedata) under the specified component id. 
</p>If any entry with the same id exists, it is overriden. The caller is responsible for providing correctly serialized content.

!!! info "In-Memory only"
    This method does not trigger a write-to-disk operation. Call the [RequestGameSave](#requestgamesave) method to write the updated data to disk.

``` cs title="C#"
string newData = JsonSerializer.Serialize(Actor.Transform);
SaveManager.Instance.SetSaveData(ID, newData);

```

</br>


### [GetSaveData](SaveManager.md#getsavedata)
Provides thread-safe read access to the savegame data for a specific component. This method reads the in-memory save state without loading a save from disk.

|Parameter|Type|Description|
|----|---|---|
|id|Guid|Unique identifier of a save component|

|Returns|Description|
|----|---|
|string|The data associated with the component as a serialized JSON-string|

**How it works**: This method retrieves the serialized data associated with a specific component id from the [ActiveSaveData](#activesavedata). 
</p>The caller is responsible for deserializing the string to it's original contents. If no entry exists for the given ID, the method throws a `KeyNotFoundException`.

!!! info "In-Memory only"
    This method does not trigger a read-from-disk operation. Call the [RequestGameLoad](#requestgameload) method to read serialized data from disk.

``` cs title="C#"
string data = SaveManager.Instance.GetSaveData(ID);

if(data != null)
    Transform savedTransform = JsonSerializer.Deserialize<Transform>(data);

```

</br>



### [RequestGameSave](SaveManager.md#requestgamesave)
Queues a request to save the current game state to the disk. To prevent performance issues, the save operation runs on a background thread.

|Parameter|Type|Description|
|----|---|---|
|savegameName|string|The friendly name shown in a load menu|
|customMetaData|object|(Optional) Any extra data you want to attach to the [SaveMeta](SaveMeta.md) (like player progress or current quest) for display menus|

**How it works**: This method uses a queue system. If you call this method while a save or load operation is already in progress, the request will be queued and executed as soon as possible.

!!! info "Anti-Spam Protection"
    The queue only holds _one_ save request at a time. If a player "hammers" the save button while the system is busy, only the most recent request will be executed once the current task finishes. This keeps the disk and performance clean.

``` cs title="C#"
// Simple save with a custom name
SaveManager.Instance.RequestGameSave("Before the boss");

// Save with custom metadata (i.e. for your Load Menu)
var meta = new MyProjectMeta { PlayerLevel = 5, CurrentQuest = "Dungeon Boss" };
SaveManager.Instance.RequestGameSave("AutoSave", meta);

```
</br>


### [RequestGameLoad](SaveManager.md#requestgameload)
Queues a request to load a specific savegame from disk. To prevent performance issues, the load operation runs on a background thread.

|Parameter|Type|Description|
|----|---|---|
|saveName|string|Unique identifier of the save slot (typically a GUID-like string)|


**How it works**: This method uses a queue system. If you call this method while a save or load operation is already in progress, the request will be queued and executed as soon as possible.

!!! tip "`SaveName` vs. `DisplayName`"
    The names of save files are guid strings and are stored in the corresponding [SaveMeta](SaveMeta.md) as the `SaveName` property. The `DisplayName` property, on the other hand, contains the friendly name shown in a load menu.

``` cs title="C#"
string saveName = SaveManager.Instance.SaveMetas[0].SaveName;
SaveManager.Instance.RequestGameLoad(saveName);

```
</br>

### [RequestGameDelete](SaveManager.md#requestgamedelete)
Queues a request to delete a specific savegame from disk. This operation runs on a background thread.

|Parameter|Type|Description|
|----|---|---|
|saveName|string|Unique identifier of the save slot (typically a GUID-like string)|

**How it works**: This method uses a queue system. If you call this method while a save or load operation is already in progress, the request will be queued and executed as soon as possible.

!!! danger "Irreversible Operation"
    Deleting a savegame permanently removes both the save file and the [SaveMetas](#savemetas). Proceed with caution. Consider promting the player for confirmation.
    The operation is irreversible unless the user manually restores the file and [SaveMetas](#savemetas) entry. 

</br>

### [RequestAssetsSave](SaveManager.md#requestassetssave)
Queues a request to save all assets defined in [FlaxSaveSettings](FlaxSaveSettings.md#assets) to disk. To prevent performance issues, the save operation runs on a background thread.

|Parameter|Type|Description|
|----|---|---|
|(none)|||

**How it works**: This method uses a queue system. If you call this method while a save or load operation is already in progress, the request will be queued and executed as soon as possible.

!!! info "Registered assets only"
    Only assets explicitly registered in the active `FlaxSaveSettings` asset are processed. Other assets are ignored. Asset saving is independent from game saves, so[SaveMetas](#savemetas) and savegames are unaffected by this method.

!!! tip
    This system is intended for engine settings, global configuration and persistent editor/runtime assets.

</br>

### [RequestAssetsLoad](SaveManager.md#requestassetsload)
Queues a request to load the states of all assets defined in [FlaxSaveSettings](FlaxSaveSettings.md#assets) from disk. To prevent performance issues, the loading operation runs on a background thread.

|Parameter|Type|Description|
|----|---|---|
|(none)|||

**How it works**: This method uses a queue system. If you call this method while a save or load operation is already in progress, the request will be queued and executed as soon as possible. 
</p>During execution, the system will iterate over all asset references registered in [FlaxSaveSettings](FlaxSaveSettings.md#assets) and overwrites their current in-memory state with the serialized data from disk.

**In-Game behaviour**: This method is automatically executed during systme initialization, to ensure that engine settings and global configuration is available as soon as possible.

**Editor behaviour**: By default, this method is not executed automatically in the editor to prevent accidental permanent modification of asset files. Asset loading in the editor can be explicitly enabled in [FlaxSaveSettings](FlaxSaveSettings.md#skiploadingsettingsineditor).

!!! info "Registered assets only"
    Only assets explicitly registered in the active `FlaxSaveSettings` asset are processed. Other assets are ignored. Asset loading is independent from game saves, so[SaveMetas](#savemetas) and savegames are unaffected by this method.

!!! warning "Editor data loss risk"
    Loading assets overwrites the current asset state. In the editor, this permanently modifies project assets. Proceed with caution.

</br>



### [RemoveSaveData](SaveManager.md#removesavedata)
Provides thread-safe removal of a specific entry from the active in-memory savegame data. The removal is applied to disk during the next save operation.

|Parameter|Type|Description|
|----|---|---|
|id|Guid|Unique identifier of a save component|

**How it works**: This method removes the serialized data associated with the specific component id from the [ActiveSaveData](#activesavedata). If no entry exists for the given id, the call has no effect.

!!! info "Forgotten, but not gone"
    A component might re-add itself to [ActiveSaveData](#activesavedata) during the [OnSaving](#onsaving) event or via [SetSaveData](#setsavedata). Ensure the component is disabled or removed if permanent deletion is intended.

!!! info "In-Memory only"
    This method only affects the current active in-memory save state. Call the [RequestGameSave](#requestgamesave) method to write the updated data to disk.

</br>

### [ClearSaveData](SaveManager.md#clearsavedata)
Provides thread-safe for clearing all entries from the active in-memory savegame data.

|Parameter|Type|Description|
|----|---|---|
|(none)|||

**How it works**: This method clears all serialized component data from [ActiveSaveData](#activesavedata). Unsaved runtime progress is lost, unless a save is performed before clearing. 
</p> This method does not affect existing save files. The cleared state is written to disk during the next save operation.

!!! tip "New Game / New Game+"
    This method is useful for starting a new playthrough, without restarting the application. It ensures that no persistent runtime state is carried over.

!!! warning "All unsaved progress will be lost"
    Clearing the active save data discards all unsaved progress. Consider promting the player to save the current state before calling this method.

</br>

### [SetAutoSaveActive](SaveManager.md#setautosaveactive)
Enables and disables the auto-save feature.

|Parameter|Type|Description|
|----|---|---|
|isActive|bool|The new auto-save state (`true` = enabled, `false` = disabled)|

**How it works**: This method toggles the internal auto-save system. When enabled, the `SaveManager` will trigger save operations at the configured [AutoSaveInterval](FlaxSaveSettings.md#autosaveintervalminutes). 
</p>Enabling auto-save will not immediatly trigger a save operation, but wait until the configured interval is over. Disabling auto-save does not cancel currently running save operations.

</br>

### [OpenSaveDirectory](SaveManager.md#opensavedirectory)
Opens the savegame directory in the systems file browser.

|Parameter|Type|Description|
|----|---|---|
|(none)|||

**How it works**: This method launches a new system browser window in the active directory used by FlaxSave.

!!! info "Location"
    Savegames created in the Flax Editor are located in a different directory than savegames created in a packaged game build.

</br>
