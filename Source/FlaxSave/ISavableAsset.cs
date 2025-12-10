// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

namespace FlaxSave;

/// <summary>
/// ISavableAsset interface.
/// </summary>
public interface ISavableAsset
{
    void SaveAction();
    void LoadAction();
}
