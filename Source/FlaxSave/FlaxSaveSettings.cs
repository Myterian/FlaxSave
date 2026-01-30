// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FlaxEditor.Content.Settings;
using FlaxEngine;

namespace FlaxSave;

/// <summary>Setting for the save system, like auto save interval, auto save name string, but also save paths, etc.</summary>
public class FlaxSaveSettings : FlaxEditor.Content.Settings.SettingsBase
{
    private string saveDir;
    private string settingDir;
    private string validatedFileExtension;

    [ShowInEditor, Serialize, EditorDisplay("Savable Assets"), EditorOrder(5), ExpandGroups]
    public List<JsonAssetReference<ISavableAsset>> Assets { get; private set; } = new();

    [ShowInEditor, Serialize, EditorDisplay("Meta"), EditorOrder(1), Tooltip("The file extension name for savegame files. Internally normalized to guarantee a valid file extension. Defaults to '.save' if normalization fails."),]
    private string savegameFileExtension = ".save";

    [ShowInEditor, Serialize, EditorDisplay("Meta"), EditorOrder(0), Tooltip("Can be helpful to determine outdated savegames. Format is \"Major.Minor.Build.Revision\" (you don't need to set all of them)")]
    private Version savegameVersion = new(1, 0, 0, 0);

    /// <summary>Auto save interval in minutes</summary>
    [ShowInEditor, Serialize, EditorDisplay("Auto Save"), EditorOrder(3), VisibleIf("autoSave"), Tooltip("Sets the time in-between auto saves"), Limit(min: 1)]
    private int autoSaveIntervalMinutes = 5;

    [ShowInEditor, Serialize, EditorDisplay("Auto Save"), EditorOrder(2), Tooltip("Toggles auto save on and off")]
    private bool autoSave = true;

    [ShowInEditor, Serialize, EditorDisplay("Savable Assets"), EditorOrder(4), Tooltip("Changes made to Json Assets in editor are saved permanently, even when loaded from disk in play mode. This is to not accidentally mess up any configuration.")]
    private bool skipLoadingAssetsInEditor = true;

    /// <summary>Auto save intervals in minutes</summary>
    [HideInEditor]
    public int AutoSaveIntervalMinutes => autoSaveIntervalMinutes;

    /// <summary>Auto save intervals, converted to seconds</summary>
    [HideInEditor]
    public int AutoSaveIntervalSeconds => autoSaveIntervalMinutes * 60;

    /// <summary>Auto save interval, converted to milliseconds</summary>
    [HideInEditor]
    public int AutoSaveIntervalMilliseconds => autoSaveIntervalMinutes * 60_000;

    /// <summary>A value indicating if auto saves should be active</summary>
    [HideInEditor]
    public bool AutoSave => autoSave;

    [HideInEditor]
    public bool SkipLoadingSettingsInEditor => skipLoadingAssetsInEditor;

    /// <summary>A value indicating the savegame version as defined in the save settings</summary>
    [HideInEditor]
    public Version SavegameVersion => savegameVersion;

#if FLAX_EDITOR
    /// <summary>Directory path for savegames</summary>
    [HideInEditor]
    public string SavegameDirectory => saveDir ??= Path.Combine(Globals.ProductLocalFolder, GameSettings.Load().ProductName);
#else
    /// <summary>Directory path for savegames</summary>
    [HideInEditor]
    public string SavegameDirectory => saveDir ??= Globals.ProductLocalFolder;
#endif
    

    /// <summary>Full file path to the meta file</summary>
    [HideInEditor]
    public string SavegameMetaFile => Path.Combine(SavegameDirectory, "Saves.meta");

    /// <summary>Full file path to the settings configuration file</summary>
    [HideInEditor]
    public string SettingsFile => Path.Combine(SavegameDirectory, "Settings.config");

    /*
    /// <summary>Directory path for game settings</summary>
    [HideInEditor]
    public string SettingsDirectory => settingDir ??= Path.Combine(Globals.ProductLocalFolder, "Engine");
    */

    /// <summary>Get the normalized and validated file extension as defined in the save settings</summary>
    [HideInEditor]
    public string SavegameFileExtension => NormalizedFileExtension();

    

    /// <summary>Get the full path to a save file</summary>
    /// <param name="savegameName">The file name of the save file (without file extension)</param>
    /// <returns>Full file path</returns>
    public string GetSaveFilePath(string savegameName)
    {
        StringBuilder saveFile = new(savegameName);
        saveFile.Append(SavegameFileExtension);

        return Path.Combine(SavegameDirectory, saveFile.ToString());
    }

    /*
    /// <summary>Gets the full path to a settings file</summary>
    /// <param name="settingsName">The file name of the settings file (without file extension)</param>
    /// <returns>Full file path</returns>
    public string GetSettingsFilePath(string settingsName)
    {
        StringBuilder settingsFile = new(settingsName);
        settingsFile.Append(".config");

        return Path.Combine(SettingsDirectory, settingsFile.ToString());
    }
    */


    /// <summary>Makes sure the savegameFileExtension is a valid file extension and won't throw errors</summary>
    /// <returns>Valid file extension</returns>
    private string NormalizedFileExtension()
    {
        if (validatedFileExtension != null)
            return validatedFileExtension;

        char[] invalidCharacter = Path.GetInvalidFileNameChars();
        string extension = new(savegameFileExtension.Where(x => !char.IsWhiteSpace(x) && !invalidCharacter.Contains(x)).ToArray());

        if (string.IsNullOrEmpty(extension))
            return ".save";

        if (!extension.StartsWith("."))
            extension = "." + extension;

        validatedFileExtension = extension;
        return extension;
    }

    
    /// <summary>Opens the savegame directory in the file browser</summary>
    [Button("Open Save Directory")]
    public void OpenDirectory()
    {
        Directory.CreateDirectory(SavegameDirectory);
        Platform.OpenUrl(SavegameDirectory);
    }
}
