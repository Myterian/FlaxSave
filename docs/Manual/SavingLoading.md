---
title: Saving & Loading
---

# How to create persistable Objects
This section covers the **standard workflow** for making any script or actor in your scene _persistable_. 
Whether it's the players position, health, a door's open state or an npcs inventory. 
The process always follows the same pattern.

## Core Concepts

### Subscription Model
Instead of the `SaveManager` searching thru a scene (which is slow), your scripts and actors opt-in to the save process.

- Use `OnEnable` to subscribe to `SaveManager` events
- Use `OnDisable` to unsubscribe (crucial to prevent issues)



### Identity (ID)
Every script and actor in Flax has a unique `Guid` (the `ID` property). Best practice is to use this as the "key" in the save dictionary.

This ensures that when the game loads, the data finds its way back to the exact same object, even if you have multiple instances of the same prefab.

!!! info "Prefab Safety"
    Flax instantiates every prefab with a unique `ID`. Using this property is the most robust way to identify save data and avoids prefabs overwriting each other.

---


## Step-By-Step
To get the most flexibility out of **FlaxSave**, use scripts to handle saving and loading an actor state in a self-contained fashion. But the following steps can be directly applied to custom actors.

1. **Prepare your Moduls**
    
    Make sure to add the FlaxSave dependency to the `*.Build.cs` file in any module, where you want to use FlaxSave

    ``` cs title="C#"
    options.PublicDependencies.Add(nameof(FlaxSave));
    // or
    options.PublicDependencies.Add("FlaxSave");

    ```

2. **Create a script**
    
    Like you would with any. Make sure to name it something you can easy identify.

3. **Loading Data**
    
    The `OnEnable` method in your script is a good place to recreate the previously saved state of the attached actor.

    To retieve the data from the loaded savegame, you can simply call the `GetSaveData` method.

    ``` cs title="C#" hl_lines="3"
    
    public override void OnEnable()
    {
        string savedData = SaveManager.Instance.GetSaveData(ID);

        if (string.IsNullOrEmpty(savedData))
            return;

        Transform transform = JsonSerializer.Deserialize<Transform>(savedData);
        Actor.Transform = transform;
    }
    
    ```

4. **Saving Data**

    Saving data can be handled in two distinct ways, depending on whether you want to react to a global save request or manally update the state of an object.

    </br>

    **Via the `OnSaving` event**
    
    This is the standard way. Your scripts listen to the global save command and pushes its data into the shared dictionary.

    ``` cs title="C#"
    public override void OnEnable()
    {
        // Subscribe to the saving event
        SaveManager.Instance.OnSaving += OnSaving;
    }

    private void OnSaving(Dictionary<Guid, string> savegame)
    {
        // Write the actor's current rotation to the save dictionary
        string data = JsonSerializer.Serialize(Actor.Orientation)
        savegame[ID] = data;
    }

    public override void OnDisable()
    {
        // Unsubscribe to avoid memory leaks and other issues
        SaveManager.Instance.OnSaving -= OnSaving;
    }
    
    ```

    </br>
    
    **Via `SetSaveData` (Manual updates)**

    Sometimes you want to update the save data immediatly when something happens (i.e. a player picking up an item or an actor being disabled) rather than waiting for a global save request.

    ``` cs title="C#"
    public override void OnDisable()
    {
        // Manually write the actor's current rotation to the save dictionary
        string data = JsonSerializer.Serialize(Actor.Orientation)
        SaveManager.Instance.SetSaveData(ID, data);
    }
    
    
    ```

---

## Common Setups
FlaxSave is designed to handle anything from simple floats to complex custom classes. Here are the most common patterns.

!!! tip "Example Scripts"
    **FlaxSave** contains example scripts with common setups and pattern. You'll find them in the plugin folder 
    </br>`FlaxSave > Source > FlaxSaveExamples > Scripts`

### Complex Classes or Structs
If you have multiple variables (i.e. health, stamina, mana) don't save them one by one. Create a small `struct` or `class` and save the whole object. The system will handle the JSON nesting for you.

``` cs title="C#"
public struct PlayerStats
{
    public float Health;
    public int SkillLevel;
}

// During OnSaving event
private void OnSaving(Dictionary<Guid, string> savegame)
{
    PlayerStats stats = new PlayerStats
    {
        Health = 89;
        SkillLevel = 5;
    };

    savegame[ID] = JsonSerializer.Serialize(stats);
}

// Loading during actor initalization
public override void OnEnable()
{
    string data = SaveManager.Instance.GetSaveData(ID);
    PlayerStats stats = JsonSerializer.Deserialize<PlayerStats>(data);
    ...
}
```
