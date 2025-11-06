// © 2025 byteslider UG. All rights reserved.

using FlaxEngine;

namespace FlaxSave;

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
