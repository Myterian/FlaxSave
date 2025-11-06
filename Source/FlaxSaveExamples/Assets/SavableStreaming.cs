// © 2025 byteslider UG. All rights reserved.

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
