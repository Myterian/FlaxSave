---
title: UI Integration
---

# Metadata & UI Integration
This section explains how to use the [`SaveMeta`](../Api/SaveMeta.md) class to build a Save/Load menu. You'll learn how to display save information to the player without the performance cost of loading the actual save data.


## Core Concepts

### The Metadata Snapshot
When the [`SaveManager`](../Api/SaveManager.md) initializes, it doesn't load save data, but it only retrieves the small header objects called [`SaveMeta`](../Api/SaveMeta.md).

- **Performance** Listing 100 saves is nearly instant, because the heavy savegames stay on disk.
- **Access** All discovered saves are available via [`SaveManager.Instance.SaveMetas`](../Api/SaveManager.md#savemetas)

### Built-in & Custom Metadata
A save slot usually needs more than just a name. We differentiate between

- **Default Data** Save Name, Display Name, Date/Time and the `IsAutoSave` flag
- **Custom Data** Project specifc info, like "Current Location", "Player Level", "Completion Percentage"

---

## Custom Metadata
While the default [`SaveMeta`](../Api/SaveMeta.md) provides the basics (Name, Date/Time, etc.), most games require more specific information to show in a Save-Slot UI.

FlaxSave provides a straight-forward integration to attach additional information (i.e. player location, current skill level or quest progression) to a [`SaveMeta`](../Api/SaveMeta.md) entry.

!!! info "Custom Metadata Guide"
    Checkout the [Custom Metadata Guide](CustomMeta.md) for more information.

### Defining your Metadata
To store custom information, create a simple `class` that wraps the data you want to show in your Save-Slot UI.

``` cs title="C#"
public class MySaveInfo
{
    public string LocationName;
    public int PlayerLevel;
    public float CompletionPercentage;
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
        CompletionPercentage = 42.7f
    };

    // This data is now attached to the savegames metadata
    SaveManager.Instance.RequestGameSave("SaveSlot_01", customInfo);
}
```

### Retrieving Data for the UI
When building your load menu, you can access your data directly from a [`SaveMeta`](../Api/SaveMeta.md) entry. Use the [`GetCustomData<T>()`](../Api/SaveMeta.md#getcustomdataas-t-) method to safely cast the stored object back into your data class.

``` cs title="C#"
public void BuildLoadMenu()
{
    foreach (SaveMeta meta in SaveManager.Instance.SaveMetas)
    {
        // Extract the custom data
        var customInfo = meta.GetCustomDataAs<MySaveInfo>();

        if (customInfo != null)
        {
            // Update your UI elements
            Debug.Log($"Location: {customInfo.LevelName} | Level: {customInfo.PlayerLevel}");
        }
    }
}

```

!!! info "Performance"
    Custom metadata is stored as a JSON-string within the small `.meta` file. Because this file is separate from the large savegame files, you can create rich, informative save lists with zero lag.

---

## Building UI & Notifications
In this example, we're going to build an endless, dynamic list of save-slots which are a staple of RPGs, Survival and Simulation games.

Every slot in the list is a UI Widget linked to a specific [`SaveMetas`](../Api/SaveManager.md#savemetas) entry.

!!! tip "Ready-to-use Examples"
    You can find the complete implementation in the plugin folder:

    - **Widget Prefab** in `FlaxSave > Content > Example`
    - **Logic & Scripts** in `FlaxSave > Source > FlaxSaveExamples`

### 1. Create a UI Widget
![UI Widget Setup](UI-Widget.jpg)
For this example, the UI Widget consists of a `Button`, with three `Label` children. The labels will display:

- `DisplayName`
- `SaveVersion`
- `SaveDate`

Convert the setup into a prefab.

### 2. Create the Save-Slot Logic
Create and attach a script (i.e. `SaveSlot`) to your Widget prefab. This script will fill the UI with the data from a [`SaveMeta`](../Api/SaveMeta.md) object and handle the click event, to trigger a savegame load.

``` cs title="C#"
public class SaveSlot : Script
{
    public ControlReference<Button> LoadButton;
    public ControlReference<Label> SavetTitelLabel;
    public ControlReference<Label> VersionLabel;
    public ControlReference<Label> DateLabel;

    private SaveMeta saveMeta;

    // Sets up the ui
    public void Bind(SaveMeta metaData)
    {
        saveMeta = metaData;

        // The friendly name of the savegame
        SavetTitelLabel.Control.Text = saveMeta.DisplayName;

        // Display the savegame Version
        VersionLabel.Control.Text = saveMeta.SaveVersion.ToString();

        // Display the time and date, when this savegame was created
        DateTime localTime = saveMeta.SaveDate.ToLocalTime();
        DateLabel.Control.Text = localTime.ToString("g");

        // Subscribe to the button clicked event
        LoadButton.Control.Clicked += LoadSave;
    }

    // Request a savegame load with the SaveName from the associated save meta
    public void LoadSave()
    {
        SaveManager.Instance.RequestGameLoad(saveMeta.SaveName);
    }

    public override void OnDisable()
    {
        // Don't forget to unsubscribe from events!
        LoadButton.Control.Clicked -= LoadSave;
        base.OnDisable();
    }
}

```

Remember to save changes to your prefab.

### 3. Scene Setup
![Scene Setup](UI-SceneSetup.jpg)
For the list in the UI itself, create a `UICanvas` and add a child `UIControl` with the type set to `VerticalPanel`. 

The advantage of a `VerticalPanel` is that it handles the layout for you: all children (your Save-Slot Widgets) will automatically be arranged into a clean vertical list as they are spawned. 
This keeps your UI organized and responsive without manual positioning.


### 4. Create the List Logic
Create and attach a script (i.e. `SaveListManager`) to the `VerticalPanel` control.

This script spawns the UI Widgets as children of a `VerticalPanel`. To keep the UI in sync with the [`SaveMetas`](../Api/SaveManager.md#savemetas), we use the [`OnSaved`](../Api/SaveManager.md#onsaved) and [`OnDeleted`](../Api/SaveManager.md#ondeleted) events.

``` cs title="C#"
public class SaveListManager : Script
{
    public Prefab Widget;
    public ControlReference<VerticalPanel> ListContainer;


    // Instantiates UI widgets with meta infos about savegames
    public void SpawnWidgets()
    {
        // Clear everything to avoid double entries
        Actor.DestroyChildren();

        // Instance a widget for every SaveMeta entry in the SaveManager.
        for (int i = 0; i < saveManager.SaveMetas.Count; i++)
        {
            Actor newWidget = PrefabManager.SpawnPrefab(Widget, ListContainer);
            newWidget.GetScript<SaveSlot>()?.Bind(saveManager.SaveMetas[i]);
        }
    }


    public override void OnEnable()
    {
        saveManager = SaveManager.Instance;

        // Automatically refreshes UI when as save is created or deleted
        SaveManager.Instance.OnSaved += SpawnWidgets;
        SaveManager.Instance.OnDeleted += SpawnWidgets;

        // Initial build of the UI
        SpawnWidgets();
    }

    public override void OnDisable()
    {
        // Don't forget to unsubscribe your methods from events!
        SaveManager.Instance.OnSaved -= SpawnWidgets;
        SaveManager.Instance.OnDeleted -= SpawnWidgets;
    }
}

```

Whenever you request a game save or an auto-save happens, the Save-Slot UI will automatically update and you can click the `Button` of the UI Widget to load a previous game state.



---



## Best Practices

### Notifications
Since data collection happens in the background, trying to show a a "Game Saved!" notification directly inside `OnSaving` will be rejected by the engine. 

Use the [`InvokeOnSaved`](../Api/SaveManager.md#invokeonsaved) helper to trigger notifications,
as the helper waits for the background work to finish, before executing any action on the main-thread. 

The same concept goes for [`InvokeOnLoaded`](../Api/SaveManager.md#invokeonloaded) and [`InvokeOnDeleted`](../Api/SaveManager.md#invokeondeleted).

``` cs title="C#"
public void QuickSave()
{
    // This ensures the notification happens on the UI thread
    // only AFTER the file is safely written to disk.
    SaveManager.Instance.InvokeOnSaved(() => {
        Debug.Log("Game Saved successfully!");
    });

    SaveManager.Instance.RequestGameSave("QuickSave");
}

```

### UI Refreshing
Don't refresh your UI list every frame. Only rebuild the list, when something actually changes. By subscribing to the [`SaveManager events`](../Api/SaveManager.md#events), 
your UI stays reactive without wasting performance.

- `OnSaved`: Refresh, when a new save is created.
- `OnDeleted`: Refresh, when a save is removed (to avoid ghost-slots)
- `OnLoaded`: Close the menu and trigger scene transition, once the data is ready


### Localization
Use `Localization Keys` in the [`SaveMeta.DisplayName`](../Api/SaveMeta.md#displayname) for automated entries, like auto-saves and quick-saves. By using a `LocalizationString`, you can attempt to translate these keys, while still gracefully falling back to the raw string, if the user provided a custom name for a manual save.

``` cs title="C#"
public void Bind(SaveMeta metaData)
{
    // This tries to return the localized version of DisplayName.
    // If no localization key matches, it falls back to the raw value.
    var displayText = new LocalizationString()
    { 
        Id = metaData.DisplayName, 
        Value = metaData.DisplayName 
    };

    SavetTitelLabel.Control.Text = displayText;
}

```

</br>

When displaying the [`SaveDate`](../Api/SaveMeta.md#savedate), use the C# built-in formatting and `ToLocalTime`, to respect the user's regional settings and time zone.

``` cs title="C#"
// Simple. Uses the general date/time pattern (i.e. 01/01/2026 3:00 PM)
DateLabel.Control.Text = metaData.SaveDate.ToLocalTime().ToString("g");

// Advanced. Full control using the games current culture setting
DateTime localTime = metaData.SaveDate.ToLocalTime();

string date = localTime.ToString("d", Localization.CurrentCulture);
string time = localTime.ToString("t", Localization.CurrentCulture);

DateLabel.Control.Text = date + ", " + time;

```


### Empty Saves
Always handle the "No Saves Found" cases. If `SaveManager.Instance.SaveMetas.Count == 0`, display a simple label saying "No saves found!". It's a small detail that makes the UI feel finished.