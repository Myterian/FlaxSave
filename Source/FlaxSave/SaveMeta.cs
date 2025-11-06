// © 2025 byteslider UG. All rights reserved.

using System;

namespace FlaxSave;

/// <summary>SavegameMeta class.</summary>
public class SaveMeta
{
    public string SaveName { get; init; }
    public string DisplayName { get; init; }
    public Version SaveVersion { get; init; }
    public DateTime SaveDate { get; init; }

    public object CustomData { get; init; }
    public bool IsAutoSave { get; init; }

    /// <summary>Converts the stored CustomData safely into a type</summary>
    /// <typeparam name="T">The type to convert to</typeparam>
    /// <returns>Converted CustomData as T. Returns default of T if failed to convert</returns>
    public T GetCustomDataAs<T>()
    {
        if (CustomData is T value)
            return value;

        return default;
    }
}
