// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.Json;
using FlaxSave;

namespace FlaxSaveExamples;

/// <summary>Example script that saves and loads player data</summary>
public class SavablePlayerStats : Script
{
    // This example demonstrates a save setup for how multiple values (name, position, skill level, health)
    // can be stored into a single savegame entry.
    //
    // Loading data from a savegame is best done during the scripts initilization (OnEnable or OnStart).
    // At this point the savegame is should already loaded and available via the SaveManager.
    // This approach ensures that dynamically spawned objects can restore their state, because
    // no loading events are missed due to timing or lifecycle order.
    //
    // Saving, on the other hand, is event driven and happens during SaveManager.OnSaving,
    // right before the savegame is written to disk.
    //
    // Fun fact: Without the comments, this scripts would be ~65 lines long.


    /// <summary>A wrapper class is used to bundle the various values, so they can be saved together</summary>
    private class PlayerStats
    {
        public string PlayerName;
        public Vector3 Position;
        public int SkillLevel;
        public int Health;
    }

    /// <summary>Serializes the current player stats and writes them to the savegame</summary>
    /// <param name="savegame">The active savegame data container provided by the SaveManager.OnSaving event</param>
    public void SaveAction(Dictionary<Guid, string> savegame)
    {
        // Step one is to create the wrapper class for the save data and set all relevant values 
        PlayerStats stats = new();

        stats.PlayerName = Actor.Name;
        stats.Position = Actor.Position;
        stats.SkillLevel = 42;
        stats.Health = 100;

        // Savegame files are Json-based, so the PlayerStats are serialized to json
        string data = JsonSerializer.Serialize(stats);

        // Add or update the savegame entry for this object using its unique ID
        savegame[ID] = data;
    }


    public override void OnEnable()
    {
        base.OnEnable();

        // Subscribe to the save event. This event is dispatched during manual or auto-save,
        // when all active objects are asked to provide their save data.
        SaveManager.Instance.OnSaving += SaveAction;

        // Load state from the currently loaded savegame. At this point the savegame should
        // already be loaded and stored inside the SaveManager.
        string savedData = SaveManager.Instance.GetSaveData(ID);

        if (string.IsNullOrEmpty(savedData))
            return;

        // Savegame data is stored as a json string, which needs to be converted back 
        PlayerStats savedStats = JsonSerializer.Deserialize<PlayerStats>(savedData);

        // Final step: Restore the player's state
        Actor.Name = savedStats.PlayerName;
        Actor.Position = savedStats.Position;
        int level = savedStats.SkillLevel;
        int health = savedStats.Health;
    }


    public override void OnDisable()
    {
        base.OnDisable();

        // Don't forget to unsubscribe your methods from events!
        SaveManager.Instance.OnSaving -= SaveAction;


        // The next section is optional, but good practice: 
        // Write the latest state to the savegame, when the object is disabled. This ensures that the savegame
        // remains up-to-date even if the object no longer exists at the time of the next save.


        // During engine shutdown, serialization may not be reliable.
        // For this example, we just skip manual saving. 
        if (Engine.IsRequestingExit)
            return;

        // Manually update the savegame entry for this object
        PlayerStats stats = new();

        stats.PlayerName = Actor.Name;
        stats.Position = Actor.Position;
        stats.SkillLevel = 42;
        stats.Health = 100;

        // Savegame files are Json-based, so the PlayerStats are serialized to json
        string data = JsonSerializer.Serialize(stats);
        SaveManager.Instance.SetSaveData(ID, data);
    }
}
