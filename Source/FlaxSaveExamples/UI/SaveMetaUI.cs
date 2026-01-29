// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Text;
using FlaxEngine;
using FlaxEngine.GUI;

namespace FlaxSave;

/// <summary></summary>
public class SaveMetaUI : Script
{
    public ControlReference<Label> SavetTitelLabel;
    public ControlReference<Label> VersionLabel;
    public ControlReference<Label> DateLabel;


    /// <summary>
    /// Sets the UI Label text with the data from a SaveMeta instance. 
    /// This method is called by the <see cref="SavegameList"/> script, when instancing UI widgets.
    /// </summary>
    /// <param name="metaData">The SaveMeta instance to read from</param>
    public void Bind(SaveMeta metaData)
    {
        // Display the savegame fiendly name as a Label text
        if (metaData.IsAutoSave)
            SavetTitelLabel.Control.Text = "Auto-Save";
        else
            SavetTitelLabel.Control.Text = metaData.DisplayName;


        // Display the savegame Version as a Label text
        VersionLabel.Control.Text = metaData.SaveVersion.ToString();


        // Display the time and date, when this savegame was created as a Label text
        DateTime localTime = metaData.SaveDate.ToLocalTime();
        StringBuilder dateBuilder = new();

        dateBuilder.AppendJoin(", ", localTime.ToString("d", Localization.CurrentCulture), localTime.ToString("t", Localization.CurrentCulture));
        DateLabel.Control.Text = dateBuilder.ToString();

    }
}
