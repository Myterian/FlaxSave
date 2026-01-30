---
title: ISavableAsset
---

# interface ISavableAsset
The `ISavable` interface allows you to connect JSON assets directly to the save system. This is ideal for global data that should persist across different levels and sessions, not bound to any actors or scenes.

Any class implementing this interface and registered in your [FlaxSaveSettings](../Manual/SaveSettings.md) will be automatically managed by the [SaveManager](SaveManager.md).


## Methods

### [SaveAction](ISavableAsset.md#saveaction)
This method is called immediately before the assets instance is serialized and written to disk.

**Purpose**: Use this to prepare your asset instance. It's the place, where you pull live values from your game and store them in the assets variables, so they are up-to-date for saving.

</br>

### [LoadAction](ISavableAsset.md#loadaction)
This methods is called immedialty after the assets instance has successfully been loaded from disk.

**Purpose**: Use this to distribute your data. Once the values has been loaded into the asset, you can use this method to update other systems or engine settings with the new values.

</br>

---

## How it works
When the `SaveManager` initializes, it automatically calls [`RequestAssetLoad`](SaveManager.md#requestassetsload) for all registered assets in the [SaveSettings](../Manual/SaveSettings.md). This ensures your global settings are ready before the first scene is loaded.

!!! info "Editor protection"
    To prevent accidental data loss while working in the Flax Editor, automatic assets loading is disabled by default during edit-time. This behaviour can be configured in [SaveSettings asset](../Manual/SaveSettings.md)


### [Deep Serialization](ISavableAsset.md#deep-serialization)
An important aspect of `ISavable` asset is that the entire asset instance is serialized and deserialized. With that, public fields and properties are directly saved to disk. During loading, the active state of the asset instance is replaced with the loaded data from disk.

You can simply define any important configuration data as field or property in your class and the rest is handled by the save system.

### [Examples](ISavableAsset.md#examples)
The following example class can be used as JSON asset and registered in the [SaveSettings](../Manual/SaveSettings.md).

``` cs title="C#"
using FlaxEngine;

namespace FlaxSave;

public class SavableGraphics : ISavableAsset
{
    // Important configuration data for the game
    public Float2 ScreenSize;
    public bool UseVSync;

    // Pull the current screen settings from engine,
    // before everything is written to disk
    public void SaveAction()
    {
        UseVSync = Graphics.UseVSync;
        ScreenSize = Screen.Size;
    }

    // Apply settings to the engine, after everything
    // was loaded from disk
    public void LoadAction()
    {
        Graphics.UseVSync = UseVSync;
        Screen.Size = ScreenSize;
    }
}
```

!!! tip "No manual parsing"
    Contrary to savegames, assets don't need handle their data serialization. `public` fields and properties are automatically handled by the system.