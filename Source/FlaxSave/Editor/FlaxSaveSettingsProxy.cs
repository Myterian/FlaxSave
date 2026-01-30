// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

#if FLAX_EDITOR

using System;
using FlaxEditor.Content;
using FlaxEngine;

namespace FlaxSave;

[HideInEditor] // Hides this class in the ui control type selection, because it shows up there for some fucky reason
public class FlaxSaveSettingsItem : JsonAssetItem
{
    public FlaxSaveSettingsItem(string path, Guid id, string typeName) : base(path, id, typeName)
    {
        Guid iconAssetId = FlaxEngine.Json.JsonSerializer.ParseID("e46cc713441dd3d40ad297925e3726e8");
        SpriteAtlas texture = Content.LoadAsync<SpriteAtlas>(iconAssetId);

        if (!texture || texture.WaitForLoaded())
            return;

        _thumbnail = texture.FindSprite("Default");
    }
}

[ContentContextMenu("New/FlaxSave/Save Settings")]
public class FlaxSaveSettingsProxy : SpawnableJsonAssetProxy<FlaxSaveSettings>
{
    public override string NewItemName => "Save Settings";

    public override AssetItem ConstructItem(string path, string typeName, ref Guid id)
    {
        return new FlaxSaveSettingsItem(path, id, typeName); ;
    }
}
#endif