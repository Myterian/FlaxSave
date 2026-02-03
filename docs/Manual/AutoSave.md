---
title: Custom Auto-Save
---

# Auto-Saves in Production Workflow
The built-in auto-save feature is perfect for prototyping, but game productions often require more control about timing and data. 

You might want to attach project-specific metadata to an auto-save, change the save interval, or trigger auto-saves at specific Checkpoints.

In this section, we'll take a look on how to create a custom auto-save feature, that suits project-specific needs.

## Core Concepts

### Timers & Triggers
You can use two approaches to **auto-save timers**.

- You can keep the [`SaveSettings`](SaveSettings.md) interval for auto-saves, but listen to a "Can I save now?" condition in your games logic.
- Ignore any `SaveSettings` and create an internal timer in your games logic. This provides full control and customizablilty via the games UI.

For **triggers** (i.e. Checkpoints), you can simply call [`RequestGameSave`](../Api/SaveManager.md#requestgamesave), when entering a trigger volume.


### The Auto-Save Flag
When calling [`RequestGameSave`](../Api/SaveManager.md#requestgamesave) via the game logic, the [`IsAutoSave`](../Api/SaveMeta.md#isautosave) will never be `true`.
This is because the built-in [`IsAutoSave`](../Api/SaveMeta.md#isautosave) can only be written to by the built-in auto-save feature. Use the [Custom Metadata](#custom-metadata-for-auto-saves) approach in the next section, to handle your custom games auto-save flags.

---

## Custom Metadata for Auto-Saves
Just like manual saves, auto-saves benefit greatly from custom metadata. Storing a **quest-title** or a **location name** helps players recognize where they are returning to.

FlaxSave provides a straight-forward integration to attach additional information to a [`SaveMeta`](../Api/SaveMeta.md) entry.

!!! info "Custom Metadata Guide"
    Checkout the [Custom Metadata Guide](CustomMeta.md) for more information.

### Defining your Metadata
To store custom information, create a simple `class` that wraps the data you can to attach to your saves.

``` cs title="C#"
public class MySaveInfo
{
    public string LocationName;
    public int PlayerLevel;
    public bool AutoSave;
}

```

### Attaching Data during Save
When you request a save, you can pass your custom `class` object directly into the [`RequestGameSave`](../Api/SaveManager.md#requestgamesave) method. The [`SaveManager`](../Api/SaveManager.md) will serialize the `class` and store it inside the [`SaveMeta`](../Api/SaveMeta.md) file.

``` cs title="C#"
public void SaveTheGame()
{
    var customInfo = new MySaveInfo()
    {
        LevelName = "The Darkest Dungeon",
        PlayerLevel = 5,
        AutoSave = true
    };

    // This data is now attached to the savegames metadata
    SaveManager.Instance.RequestGameSave("My Save", customInfo);
}
```

### Checkout custom Data
You can access your data directly from a [`SaveMeta`](../Api/SaveMeta.md) entry. Use the [`GetCustomData<T>()`](../Api/SaveMeta.md#getcustomdataas-t-) method to safely cast the stored object back into your data class.

``` cs title="C#"
public void BuildLoadMenu()
{
    foreach (SaveMeta meta in SaveManager.Instance.SaveMetas)
    {
        // Extract the custom data
        var customInfo = meta.GetCustomDataAs<MySaveInfo>();

        if (customInfo != null)
            Debug.Log($"Is Auto Save: {customInfo.AutoSave} | Level: {customInfo.PlayerLevel}");
        
    }
}

```

!!! info "Performance"
    Custom metadata is stored as a JSON-string within the small `.meta` file. Because this file is separate from the large savegame files, you can create rich, informative save lists with zero lag.

---

## Step-By-Step

### Disable the Auto-Save
In your [`SaveSettings`](SaveSettings.md), uncheck `Auto Save`. The `SaveManger` will now remain idle until your game logic explicity requests a save.

### Create Timer Logic
If you want to keep the convinience of a timer, but add custom information to a [`SaveMeta`](../Api/SaveMeta.md) entry, you can implement a simple controller script.

``` cs title="C#"
public class AutoSaveController : Script
{
    public float SaveIntervalMinutes = 5.0f;
    private float timer;

    public override void OnUpdate()
    {
        timer += Time.DeltaTime;

        if (SaveIntervalMinutes * 60 <= timer)
        {
            if (CanSaveNow())
            {
                TriggerAutoSave();
                timer = 0;
            }
        }
    }

    private bool CanSaveNow()
    {
        // Add your custom logic here
        // i.e. check if the player is in combat or a dialogue
        return !Player.Instance.IsInCombat;
    }

    private void TriggerAutoSave()
    {
        // UI Notification, which will show AFTER the game is saved
        SaveManager.Instance.InvokeOnSaved(() => {
            Debug.Log("Game Saved!");
        });

        var info = new MySaveInfo() 
        { 
            AutoSave = true, 
            LocationName = "Wilderness" 
        };

        SaveManager.Instance.RequestGameSave("Auto-Save", info);
    }
}

```

!!! info "Persistent Auto-Save Controller"
    Consider using the Flax Engines `GamePlugin` type, instead of `Script` for a session persistent auto-save controller.


### Create Checkpoint Logic
Create a script that detects the player and tells the `SaveManger` to performa a save with your custom metadata.

``` cs title="C#"
public class CheckpointTrigger : Script
{
    public string AreaName = "The Dungeon Entrance";
    private bool alreadyTriggered = false;

    public void OnTriggerEnter(PhysicsColliderActor collider)
    {
        // Ensures the player only triggers once
        if(alreadyTriggered == true || collider is not CharacterController)
            return;

        alreadyTriggered = true;

        var myInfo = new MySaveInfo()
        {
            LocationName = AreaName,
            AutoSave = true
        };

        // Creates a savegame with the custom info
        SaveManager.Instance.RequestGameSave("Auto-Save", myInfo);
    }

    public override void OnEnable()
    {
        // Register for the trigger enter event
        Actor.As<Collider>().TriggerEnter += OnTriggerEnter;
    }

    public override void OnDisable()
    {
        // Don't forget to unsubscribe from events!
        Actor.As<Collider>().TriggerEnter -= OnTriggerEnter;
    }
}

```

---

## Best Practices

- **Player State** Before triggering an auto-save, check if the player is in a "safe" state. Avoid saving, while the player is falling into a pit or in the middle of a death animation, to prevent death loops.
- **Save Throttling** If you use a timer-based approch, ensure you don't save during level load loading or during conversation with NPCs.
- **Visual Feedback** Use the [`InvokeOnSaved`](../Api/SaveManager.md#invokeonsaved) method, to display a subtle "Checkpoint Reached" or "Game Saved" notification in your UI, to let players know their progress is secure.