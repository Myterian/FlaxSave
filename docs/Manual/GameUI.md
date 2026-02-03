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
        CompletionPercentage = 42.7f;
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
<!-- Image of finished widget in prefab editor -->
For this example, the UI Widget consists of a `Button`, with three `Label` children. The labels will display:

- `DisplayName`
- `SaveVersion`
- `SaveDate`

Convert the setup into a prefab.

### 2. Create the Save-Slot Logic
Create and attach a script (i.e. `SaveSlot`) to your Widget. This script will fill the UI with the data from a [`SaveMeta`](../Api/SaveMeta.md) object and handle the click event, to trigger a savegame load.

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
        SaveManager.Instance.RequestGameLoad(saveName);
    }

    public override void OnDisable()
    {
        // Don't forget to unsubscribe from events!
        LoadButton.Control.Clicked -= LoadSave;
        base.OnDisable();
    }
}

```

### 3. Scene Setup
<!-- Create UICanvas and Vertical Panel control, attach script and set widget field -->


### 4. Create the List Logic
Create and attach a script (i.e. `SaveListManager`) to the `VerticalPanel` control.

This script spawns the UI Widgets as children of a `UIControl`. To keep the UI in sync with the [`SaveMetas`](../Api/SaveManager.md#savemetas), we use the [`OnSaved`](../Api/SaveManager.md#onsaved) and [`OnDeleted`](../Api/SaveManager.md#ondeleted) events.

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

Whenever you request a game save or an auto-save happens, the Save-Slot UI will automatically update and you can click the `Button` to recreate a previous game state.


### Notifications
Sometimes you don't want to refresh a whole list, but just trigger a quick "Toast" notification (i.e. "Game Saved!"). Since saving happens on a background thread, you cannot trigger UI changes directly from `OnSaving`.

Use the Invoke helpers to safely trigger main-thread UI actions:

``` cs title="C#"
public void QuickSave()
{
    // This ensures the notification happens on the UI thread
    // only AFTER the file is safely written to disk.
    SaveManager.Instance.InvokeOnSaved(() => {
        NotificationUI.Show("Game Saved successfully!");
    });

    SaveManager.Instance.RequestGameSave("QuickSave");
}

```


---

## Advanced UI Work

---

## Best Practices