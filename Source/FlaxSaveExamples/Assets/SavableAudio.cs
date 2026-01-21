// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using FlaxEngine;
using FlaxSave;

namespace FlaxSaveExamples;

/// <summary>
/// SavableAudio class.
/// </summary>
public class SavableAudio : ISavableAsset
{
    public bool EnableHRTF;
    
    public void SaveAction()
    {
        EnableHRTF = Audio.EnableHRTF;
    }

    public void LoadAction()
    {
        Audio.EnableHRTF = EnableHRTF;
    }
}
