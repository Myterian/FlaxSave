// © 2025 byteslider UG. All rights reserved.

using FlaxEngine;

namespace FlaxSave;

/// <summary>
/// SavegameList Script.
/// </summary>
public class SavegameList : Script
{
    public Prefab Widget;

    private SaveManager saveManager;


    public override void OnEnable()
    {
        saveManager = SaveManager.Instance;

        saveManager.OnSaved -= SpawnWidgets;
        saveManager.OnSaved += SpawnWidgets;
        
        SpawnWidgets();
    }

    public override void OnDisable()
    {
        saveManager.OnSaved -= SpawnWidgets;
    }

    public void SpawnWidgets()
    {
        Actor.DestroyChildren();

        // List<SavegameMeta> sortedMetas = new(saveManager.SaveMetas);
        // sortedMetas.Reverse();

        for (int i = saveManager.SaveMetas.Count - 1; 0 <= i; i--)
        {
            Actor newWidget = PrefabManager.SpawnPrefab(Widget, Actor);
            newWidget.GetScript<SaveMetaUI>()?.Bind(saveManager.SaveMetas[i]);
        }

        
    }
}
