// © 2025 byteslider UG. All rights reserved.

using FlaxEngine;

namespace FlaxSave;

/// <summary>
/// SaveHotbar Script.
/// </summary>
public class SaveHotbar : Script
{
    private SaveManager saveManager = null;

    /// <inheritdoc/>
    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyboardKeys.I))
            (saveManager ??= SaveManager.Instance).RequestGameSave();

        // if (Input.GetKeyDown(KeyboardKeys.O))
        //     (saveManager ??= SaveManager.Instance).RequestGameLoad();

        if (Input.GetKeyDown(KeyboardKeys.U))
            (saveManager ??= SaveManager.Instance).RequestSettingsSave();

        if (Input.GetKeyDown(KeyboardKeys.Spacebar))
            (saveManager ??= SaveManager.Instance).RequestSettingsLoad();

        if (Input.GetKeyDown(KeyboardKeys.T))
            (saveManager ??= SaveManager.Instance).OpenSaveDirectory();
    }
}
