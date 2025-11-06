// © 2025 byteslider UG. All rights reserved.

using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.Json;

namespace FlaxSave;

/// <summary>Saves and Loads transform data of an actor</summary>
public class SavableTransform : Savable
{
    // Used for comparison of transforms in the SaveCondition method.
    [ShowInEditor, ReadOnly] private Transform saveDataTransform = Transform.Identity;


    /// <summary>Serializes relevant data and adds it to the savegame</summary>
    /// <param name="savegame">Active savegame data</param>
    public override void SaveAction(Dictionary<Guid, string> savegame)
    {
        // Skip if transform hasn't changed
        if (!SaveCondition())
            return;

        // Create new data
        saveDataTransform = Actor.Transform;
        string data = JsonSerializer.Serialize(saveDataTransform);

        // Here we add (or update) the data we want to save to the concurrent dictionary. 
        // A concurrent dictionary like a regular dictionary with a <key, value> pair, but with an extra func to get the data.
        // 
        // 1.   ID is the ID of this Flax Object and the key in the dict. Flax assignes unique ids to every FlaxObject. ID is undeniably this script.
        // 2.   data is the json data we want to save and the value in the dict. Here it's the transform of this actor.
        // 3.   Third parameter is a func(Guid, string) that either adds or updates an existing key (Guid) with a value (string).
        //      For custom save components, copy and pasting this should be fine.
        // savegame.AddOrUpdate(ID, data, (key, value) => data);
        savegame[ID] = data;
    }

    /// <summary>Gets relevant data from the active savegame</summary>
    /// <param name="savegame">The savegame to read form</param>
    public override void LoadAction(Dictionary<Guid, string> savegame)
    {
        // Make sure we have data saved
        if (!savegame.ContainsKey(ID))
            return;

        // Deserialize transform from dictonary
        string data = savegame[ID];
        Transform newTransform = JsonSerializer.Deserialize<Transform>(data);

        // Apply
        Actor.Transform = newTransform;
        saveDataTransform = newTransform;
    }

    /// <summary>Condition for when to save or skip</summary>
    /// <returns>true if we should save, false otherwise</returns>
    public override bool SaveCondition()
    {
        // Check if the transform has changed
        return !Actor.Transform.Equals(saveDataTransform);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        // Optional, but good practice: Set the data in the savegame for this object, before it gets disabled.
        // This will make sure the savegame contains the latest data next time we save, even if this object doesn't exist anymore.

        // JsonSerializer is not reliably available during shutdown
        if (Engine.IsRequestingExit)
            return;

        SaveManager.Instance.SetSaveData(ID, JsonSerializer.Serialize(Actor.Transform));
    }


}
