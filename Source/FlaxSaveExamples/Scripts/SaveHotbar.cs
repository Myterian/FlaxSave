// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using FlaxEngine;

namespace FlaxSave;

/// <summary>Example for quick-save and quick-load setup</summary>
public class SaveHotbar : Script
{
    private SaveManager saveManager = null;

    /// <inheritdoc/>
    public override void OnUpdate()
    {
        // Saves the current game state
        if (Input.GetKeyDown(KeyboardKeys.F7))
            saveManager.RequestGameSave();

        // Loads the latest savegame and reloads all scenes. Savegames have to be loaded
        // before everything else. Make sure the savegame is available before
        // a script or an actor is initialized.
        if (Input.GetKeyDown(KeyboardKeys.F8))
        {
            saveManager.InvokeOnLoaded(() => ReloadScenes(Level.Scenes));
            saveManager.RequestGameLoad(saveManager.SaveMetas[^1].SaveName);
        }

        // Saves assets to disk
        if (Input.GetKeyDown(KeyboardKeys.F9))
            saveManager.RequestAssetsSave();

        // Loads assets from disk
        if (Input.GetKeyDown(KeyboardKeys.F10))
            saveManager.RequestAssetsLoad();

        if (Input.GetKeyDown(KeyboardKeys.Backspace))
            saveManager.OpenSaveDirectory();
    }

    private void ReloadScenes(Scene[] scenes)
    {        
        Level.UnloadAllScenes();

        for (int i = 0; i < scenes.Length; i++)
            Level.LoadScene(scenes[i].ID);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        saveManager = SaveManager.Instance;
    }

}
