// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using FlaxEngine;

namespace FlaxSave;

/// <summary>
/// SavableStreaming class.
/// </summary>
public class SavableStreaming : ISavableAsset
{
    public TextureGroup[] TextureGroups;

    public void SaveAction()
    {
        TextureGroups = Streaming.TextureGroups;
    }

    public void LoadAction()
    {
        Streaming.TextureGroups = TextureGroups;
    }
}
