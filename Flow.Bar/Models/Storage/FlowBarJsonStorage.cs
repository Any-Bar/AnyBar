using Flow.Bar.Extensions.Data;
using System.IO;

namespace Flow.Bar.Models.Storage;

public class FlowBarJsonStorage<T> : JsonStorage<T> where T : new()
{
    private static readonly string ClassName = "FlowBarJsonStorage";

    public FlowBarJsonStorage()
    {
        DirectoryPath = Path.Combine(DataLocation.DataDirectory(), DirectoryName);
        if (!Directory.Exists(DirectoryPath))
        {
            Directory.CreateDirectory(DirectoryPath);
        }

        var filename = typeof(T).Name;
        FilePath = Path.Combine(DirectoryPath, $"{filename}{FileSuffix}");
    }

    public new void Save()
    {
        try
        {
            base.Save();
        }
        catch (Exception)
        {

        }
    }

    public new async Task SaveAsync()
    {
        try
        {
            await base.SaveAsync();
        }
        catch (Exception)
        {

        }
    }
}
