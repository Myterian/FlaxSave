# Save Settings
The save settings asset is the central configuration hub for the FlaxSave system. It defines how and when your game data is stored.

![Settings Asset](SettingsAsset.jpg)

## Configuration Properties

|Section|Property|Description|
|---|---|---|
|**Meta**|Savegame Version|The version of your data scheme. Increment this, when you make breaking changes to your save data structure to handle migration.|
||Savegame File Extension|The file extension used for all save files.|
|**Auto Save**|Auto Save|Enables or disables the built-in auto-save feature.|
||Auto Save Intervals Minutes|How often the system triggers an auto save. Minimum: 1 Minute.|
|**Savable Assets**|Skip Loading Assets in Editor|If enabled, ISavableAssets won't load their stored values while you are in the Editor. This prevents your project assets from being permanently overwritten by accident.|
||Assets|The registry for all `ISavableAsset` instances. Drag your assets here to, so the `SaveManager` can manage them.|


## Utility Buttons

### Open Directory
Clicking this button opens your OS file browser directly at the path where your save games are currently stored.

!!! info "Editor vs. Game Saves"
    Savegames created from the editor are located at a different directory, than savegames created from a cooked game. You can use [the OpenDirectory method](../Api/FlaxSaveSettings.md#opendirectory) to always open the correct save directory.

## How to use multiple Settings
You can create multiple save settings assets for different purposes (i.e. `DebugSettings` with a 1 minute auto-save and `ReleaseSettings` with a 10 minute interval).

To switch between them, simply change the reference in your </br>`GameSettings > Custom Settings`.