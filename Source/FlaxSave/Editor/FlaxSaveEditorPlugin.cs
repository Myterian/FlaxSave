// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

#if FLAX_EDITOR

using FlaxEditor;

namespace FlaxSave;

public class FlaxSaveEditorPlugin : EditorPlugin
{
    private FlaxSaveSettingsProxy proxy;

    public override void Initialize()
    {
        base.Initialize();

        proxy = new();
        Editor.ContentDatabase.AddProxy(proxy);
    }

    public override void Deinitialize()
    {
        if (proxy != null)
            Editor.ContentDatabase.RemoveProxy(proxy);

        base.Deinitialize();
    }

    public FlaxSaveEditorPlugin()
    {
        _description = new()
        {
            Name = "FlaxSave Editor",
            Description = "Editor Assets for FlaxSave",
            Author = "Thomas Jungclaus",
            AuthorUrl = "https://github.com/Myterian/",
            RepositoryUrl = "https://github.com/Myterian/FlaxSave/",
            Category = "FlaxSave",
            IsAlpha = false,
            IsBeta = false,
            Version = SaveManager.PluginVersion
        };
    }
}
#endif
