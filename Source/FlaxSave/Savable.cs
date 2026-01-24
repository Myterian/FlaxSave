// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using FlaxEngine;

namespace FlaxSave;

/// <summary>Savable Component Base</summary>
public class Savable : Script
{
    private SaveManager saveManager = null;

    /// <summary>Serializes important data and adds it to the savegame data</summary>
    /// <param name="savegame">Active savegame data</param>
    public virtual void SaveAction(Dictionary<Guid, string> savegame)
    {
        // See examples on how to serialize data
    }

    /// <summary>Gets important data from the active savegame</summary>
    /// <param name="savegame">The savegame to read form</param>
    public virtual void LoadAction(Dictionary<Guid, string> savegame)
    {
        // See examples on how to deserialize data
    }

    /// <summary>Condition for when to save or skip</summary>
    /// <returns>bool</returns>
    public virtual bool SaveCondition()
    {
        return true;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        saveManager ??= SaveManager.Instance;

        saveManager.OnSaving -= SaveAction;
        saveManager.OnSaving += SaveAction;

        saveManager.OnLoaded -= LoadAction;
        saveManager.OnLoaded += LoadAction;

        LoadAction(saveManager.ActiveSaveData);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        saveManager ??= SaveManager.Instance;

        saveManager.OnSaving -= SaveAction;
        saveManager.OnLoaded -= LoadAction;
    }
    
}

