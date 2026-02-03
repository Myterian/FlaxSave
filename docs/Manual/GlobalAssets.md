---
title: Game Settings & Config
---

# Global Assets (ISavableAsset)
While scripts and actors handle data within a scene, `ISavableAsset` assets are designed for **global configuration and persistent project data**. These are JSON Assets that live in your content folder and stay active across scene changes and savegame states.

## Core Concepts

### Full Instance Serialization
Unlike the manual `SetSaveData` approach for scripts, `ISavableAsset` uses **Deep Serialization**. This means the `SaveManager` saves the entire state of the asset object.

- Every public field or property marked for serialization is automatically captured.
- When loading, the asset in your project is updated with the saved values.


### The "Bridge" Pattern
Because these assets persist, they act as a bridge between your game logic and the engine configuration. You should use the two interface methods to sync your data.

- `SaveAction`: Pushes the current engine state or manager variables into the asset.
- `LoadAction`: Pulls the loaded values from the asset and applies them back to the engine.

``` cs title="C#"
public class AudioSettings : ISavableAsset
{
    public bool EnableHRTF;
    
    public void SaveAction()
    {
        // Sync engine state to asset before saving to disk
        EnableHRTF = Audio.EnableHRTF;
    }

    public void LoadAction()
    {
        // Apply loaded values back to the engine
        Audio.EnableHRTF = EnableHRTF;
    }
}

```

---

## When to use Assets vs. Scripts
Choosing the right tool is key for a clean project architecture.

|Feature|Use `ISavableAsset`, when...|Use `Script/Actor`, when...|
|---|---|---|
|Scope|Data is **Global** (Settings, Achievements, Total Playtime)|Data is **Local** (Player Health, Active Quest, Door State)|
|Lifetime|Needs to be available **before** a scene loads|Only exists while a **specific scene** is active|

!!! important "Static Assets vs. Instances"
    Do **not** use `ISavableAsset` for things like `Enemy Stats` or `Quest Progession`. If you modifiy a JSON asset, the change will persist permanently thru save wipes (Savegame load, New Game, New Game+) and may cause unwanted behaviour. 
    
    For these cases (i.e. `Enemy Stats`, `Player Stats`, `Quest Progession`), always use the [script-based saving approach](SavingLoading.md) to save the state of individual json assets to the current savegame.


`ISavableAsset` is perfect for **configuration assets**. These are assets you register in your [`SaveSettings`](SaveSettings.md).

- **Config Assets** Automatically loaded on startup. Ideal for Graphics, Audio, Input, etc.
- **Scene Data** Strictly bound to the `RequestGameLoad` flow. Ideal for everything that happens inside the gameplay loop.

---

## Step-By-Step

1. **Prepare your Modules**
    
    Make sure to add the FlaxSave dependency to the `*.Build.cs` file in any module, where you want to use FlaxSave

    ``` cs title="C#"
    options.PublicDependencies.Add(nameof(FlaxSave));
    // or
    options.PublicDependencies.Add("FlaxSave");

    ```

2. **Create a Class**

    This class will act as a template for your JSON asset. Choose a clearly identifiable name.

3. **Implementation**

    Derive your class from the `ISavableAsset` interface and implement the required methods. This example handles screen resolution and v-sync.

    ``` cs title="C#"
    using FlaxEngine;
    using FlaxSave;

    public class SavableGraphics : ISavableAsset
    {
        public Float2 ScreenSize;
        public bool UseVSync;

        public void SaveAction()
        {
            // Sync engine state to asset before saving to disk
            ScreenSize = Screen.Size;
            UseVSync = Graphics.UseVSync;
        }

        public void LoadAction()
        {
            // Apply loaded values back to the engine
            Screen.Size = ScreenSize;
            Graphics.UseVSync = UseVSync;
        }
    }
    
    ```

4. **Content Asset**
    ![Create a new json asset](ISavable%20-%20create%20Json.jpg)
    Go to your projects content window and `right click > New > Json Asset`
     
    ![Select your class](ISavable%20-%20Class%20select.jpg)
    Select your class in the creation dialogue.

5. **Register your Asset**

    ![Register your `ISavableAsset` here](ISavable%20-%20Register.jpg)

    `ISavableAsset` need to be registered to be managed by the `SaveManager`. Drag your new asset in the `Assets` list within the [`SaveSettings`](SaveSettings.md).

6. **Save & Load**

    Use [`RequestAssetSave`](../Api/SaveManager.md#requestassetssave) and [`RequestAssetLoad`](../Api/SaveManager.md#requestassetsload) to save and load asset states.

    Note that [`RequestAssetLoad`](../Api/SaveManager.md#requestassetsload) is automatically executed on startup.