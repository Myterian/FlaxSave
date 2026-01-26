﻿// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.Json;
using FlaxSave;

namespace FlaxSaveExamples;

/// <summary>Saves and Loads transform data of an actor</summary>
public class SavableTransform : Script
{
    // This example demonstrates a save setup for an actor's transform.
    //
    // Loading data from a savegame is best done during the scripts initilization (OnEnable or OnStart).
    // At this point the savegame is should already loaded and available via the SaveManager.
    // This approach ensures that dynamically spawned objects can restore their state, because
    // no loading events are missed due to timing or lifecycle order.
    //
    // Saving, on the other hand, is event driven and happens during SaveManager.OnSaving,
    // right before the savegame is written to disk.
    //
    // Fun fact: Without the comments, this scripts would be ~45 lines long.


    /// <summary>Serializes the current transform and write it to the savegame</summary>
    /// <param name="savegame">The active savegame data container provided by the SaveManager.OnSaving event</param>
    public void SaveAction(Dictionary<Guid, string> savegame)
    {
        // Savegame files are Json-based, so the first step is to serialize the transform to json
        string data = JsonSerializer.Serialize(Actor.Transform);

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

        Transform savedTransform = JsonSerializer.Deserialize<Transform>(savedData);
        Actor.Transform = savedTransform;
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
        string data = JsonSerializer.Serialize(Actor.Transform);
        SaveManager.Instance.SetSaveData(ID, data);
    }
}