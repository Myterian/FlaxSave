// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Text;
using FlaxEngine;
using FlaxEngine.GUI;

namespace FlaxSave;

/// <summary></summary>
public class SaveMetaUI : Script
{
    public ControlReference<Button> LoadButton;
    public ControlReference<Label> SavetTitelLabel;
    public ControlReference<Label> VersionLabel;
    public ControlReference<Label> DateLabel;

    // Cached SaveMeta.SaveName for loading a savegame
    private string saveName;


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


        // Cache the SaveName for later RequestGameLoad operations
        saveName = metaData.SaveName;

        // Subscribe to the button clicked event
        LoadButton.Control.Clicked -= LoadSave;
        LoadButton.Control.Clicked += LoadSave;

    }

    /// <summary>Load a savegame and reload the current scenes, for changes to take effect</summary>
    public void LoadSave()
    {
        // Load the currently active scenes, after the savegame has been loaded.
        // This is to ensure that every persistable script and actor is updated.
        Scene[] scenes = Level.Scenes;
        SaveManager.Instance.InvokeOnLoaded(() =>
        {
            for (int i = 0; i < scenes.Length; i++)
                Level.LoadScene(scenes[i].ID);
        });

        // Remove every active scene before loading a savegame, as persistable actors
        // could override any previous data in the OnDisabled method
        Level.UnloadAllScenes();

        // Request a savegame load with the SaveName from the associated save meta
        SaveManager.Instance.RequestGameLoad(saveName);
    }

    public override void OnDisable()
    {
        // Don't forget to unsubscribe from events!
        LoadButton.Control.Clicked -= LoadSave;
        base.OnDisable();
    }
}
