---
title: UI Integration
---

# Metadata & UI Integration
This section explains how to use the [`SaveMeta`](../Api/SaveMeta.md) class to build a Save/Load menu. You'll learn how to display save information to the player without the performance cost of loading the actual save data.


## Core Concepts

### The Metadata Snapshot
When the [`SaveManager`](../Api/SaveManager.md) initializes, it doesn't load save states, but it only retrieves the small header objects called [`SaveMeta`](../Api/SaveMeta.md).

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

## Building a Save Slot

---

## Advanced UI Work

---

## Best Practices