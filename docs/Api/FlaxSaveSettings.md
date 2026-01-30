---
tile: FlaxSaveSettings
---

# Class FlaxSaveSettings
The `FlaxSaveSettings` class acts as the central configuration hub for the save system. It manages everything from file paths and extensions, save data versioning, to the timing logic of the auto-save feature.

`FlaxSaveSettings` is used for the [Save Settings asset](../Manual/SaveSettings.md) and configured in-editor.
## Properties



### [Assets](FlaxSaveSettings.md#assets)
A list of registered [`ISavable`](ISavableAsset.md) assets. These are automatically serialized during [`RequestAssetSave`](SaveManager.md#requestassetssave) and reloaded, when FlaxSave is initialized.

- Type: `List<JsonAssetReference<ISavableAsset>>`
- Access: `get`

!!! info "Editor protection"
    To prevent data loss, this list is not automatically loaded in editor (unless specified otherwise). Loading assets overwrites the asset state in your project. [See `SkipLoadingSettingsInEditor`](FlaxSaveSettings.md#skiploadingsettingsineditor)

!!! tip
    This list can be configured in-editor with [the Settings asset](../Manual/SaveSettings.md)

</br>

### [AutoSaveIntervalMinutes](FlaxSaveSettings.md#autosaveintervalminutes)
The auto-save interval, converted to minutes

- Type: `int`
- Access: `get`
- Note: The auto save interval can be configured in-editor with the [the Settings asset](../Manual/SaveSettings.md)

</br>

### [AutoSaveIntervalSeconds](FlaxSaveSettings.md#autosaveintervalseconds)
The auto-save interval, converted to seconds

- Type: `int`
- Access: `get`
- Note: The auto save interval can be configured in-editor with the [the Settings asset](../Manual/SaveSettings.md)

</br>

### [AutoSaveIntervalMilliseconds](FlaxSaveSettings.md#autosaveintervalmilliseconds)
The auto-save interval, converted to milliseconds

- Type: `int`
- Access: `get`
- Note: The auto save interval can be configured in-editor with the [the Settings asset](../Manual/SaveSettings.md)

</br>

### [AutoSave](FlaxSaveSettings.md#autosave)
Enables or disables the auto-save feature.

- Type: `bool`
- Access: `get`
- Note: The auto save interval can be configured in-editor with the [the Settings asset](../Manual/SaveSettings.md)

!!! info
    The auto-save feature can be switched on and off during runtime via [`SetAutoSaveActive`](SaveManager.md#setautosaveactive)

</br>

### [SkipLoadingSettingsInEditor](FlaxSaveSettings.md#skiploadingsettingsineditor)
Enables or disable automatic asset loading during system initilization in-editor. The runtime is not affected by this setting and assets are always loaded in-game.

- Type: `bool`
- Access: `get`
- Note: Auto asset loading can be configured in-editor with the [the Settings asset](../Manual/SaveSettings.md)

!!! warning "Editor data loss risk"
    If set to `false`, the system will override your projects assets with data from the save folder. In the editor, this permanently modifies project assets. Proceed with caution.

</br>

### [SavegameVersion](FlaxSaveSettings.md#savegameversion)
The current version of your projects data scheme

- Type: `bool`
- Access: `get`

!!! info "Versioning Strategy"
    It's best pratice to increment this version whenever you make breaking changes to your player data or [`ISavableAsset`](ISavableAsset.md) structures. This allows you to write checks and warn players about potential changes via UI notifications.
    
</br>


### [SavegameDirectory](FlaxSaveSettings.md#savegamedirectory)
Gets the absolute path of the save directory on disk

- Type: `string`
- Access: `get`
- Note: This directory can be opened via the [Settings asset `Open Directory` button](../Manual/SaveSettings.md)

</br>

### [SavegameMetaFile](FlaxSaveSettings.md#savegamemetafile)
Gets the absolute path of the specific file containing the [`SaveMeta`](SaveMeta.md) collection

- Type: `string`
- Access: `get`

</br>

### [SettingsFile](FlaxSaveSettings.md#settingsfile)
Gets the absolute path of the file, where global [`ISavable`](ISavableAsset.md) data is stored

- Type: `string`
- Access: `get`

</br>

### [SavegameFileExtension](FlaxSaveSettings.md#savegamefileextension)
The valid file extension used for save files, i.e `.save` or `.data`

- Type: `string`
- Access: `get`
- Note: The save file extension can be configured in-editor with the [the Settings asset](../Manual/SaveSettings.md)

</br>

---

## Methods

### [GetSaveFilePath](FlaxSaveSettings.md#getsavefilepath)
This method constructs the full and valid path to a specific save file

|Parameter|Type|Description|
|----|---|---|
|savegameName|string|The name of the save file, usualy found in a [`SaveMeta.SaveName`](SaveMeta.md#savename)|

|Returns||Description|
|---|---|
|string|The full and valid path to a specifc save file|

**How it works**: This is a convinience method, that combines the savegame path with a save file name and returns a vaild path to that file.

</br>

### [OpenDirectory](FlaxSaveSettings.md#opendirectory)
Opens the OS file explorer at the savegame directory location

|Parameter|Type|Description|
|----|---|---|
|(none)|||

**How it works**: This method opens a new system explorer window at the savegame directory.

!!! tip
    You don't need to call this code during development, the save directory can simply be opened via the [Settings asset `Open Directory` button](../Manual/SaveSettings.md)

</br>