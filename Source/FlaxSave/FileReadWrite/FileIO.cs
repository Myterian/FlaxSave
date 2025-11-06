// © 2025 byteslider UG. All rights reserved.

using System.IO;
using System.Threading.Tasks;
using FlaxEngine.Json;

namespace FlaxSave;

/// <summary>IO operations for saving and loading savegames</summary>
public class FileIO
{
    public static async Task WriteToDisk(IOOpertation io)
    {
        string data = JsonSerializer.Serialize(io.Data);
        string directory = Path.GetDirectoryName(io.Path);

        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(io.Path, data);
    }

    public static async Task<T> ReadFromDisk<T>(IOOpertation io)
    {
        if (!File.Exists(io.Path))
            return default;

        Task<string> readTask = File.ReadAllTextAsync(io.Path);
        await readTask;

        T data = JsonSerializer.Deserialize<T>(readTask.Result);
        return data;
    }

    public static void DeleteFromDisk(IOOpertation io)
    {
        if (!File.Exists(io.Path))
            return;

        File.Delete(io.Path);
    }
}
