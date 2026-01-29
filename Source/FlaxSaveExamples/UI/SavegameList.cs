// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using FlaxEngine;

namespace FlaxSave;

/// <summary>This script instanciates an UI widget, which will display meta informations of savegames</summary>
public class SavegameList : Script
{
    public Prefab Widget;


    private SaveManager saveManager;


    /// <summary>Instanciates UI widgets with meta infos about savegames as children of the current actor</summary>
    public void SpawnWidgets()
    {
        // Clear everything to avoid double entries
        Actor.DestroyChildren();


        // Instance a widget for every SaveMeta entry in the SaveManager.
        // The SaveMetaUI script will handle retrieving and displaying the meta data to the UI controls.
        for (int i = 0; i < saveManager.SaveMetas.Count; i++)
        {
            Actor newWidget = PrefabManager.SpawnPrefab(Widget, Actor);
            newWidget.GetScript<SaveMetaUI>()?.Bind(saveManager.SaveMetas[i]);
        }

        // TIP: Traverse the SaveMetas list in reverse order, to get the newest entry on top of the ui
        // for (int i = saveManager.SaveMetas.Count - 1; 0 <= i; i--)
        // {
        //     Actor newWidget = PrefabManager.SpawnPrefab(Widget, Actor);
        //     newWidget.GetScript<SaveMetaUI>()?.Bind(saveManager.SaveMetas[i]);
        // }
    }


    public override void OnEnable()
    {
        saveManager = SaveManager.Instance;

        // Subscribe to the OnSaved event, because everytime the game is saved, the SaveMetas list changes
        // and this will keep the UI updated
        saveManager.OnSaved -= SpawnWidgets;
        saveManager.OnSaved += SpawnWidgets;

        // Initial build of the UI
        SpawnWidgets();
    }

    public override void OnDisable()
    {
        // Don't forget to unsubscribe your methods from events!
        saveManager.OnSaved -= SpawnWidgets;
    }

    
}
