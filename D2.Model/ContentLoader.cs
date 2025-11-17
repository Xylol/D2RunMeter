namespace D2.Model;

public class ContentLoader : IContentLoader
{
    public byte[] GetSaveGameContent(string fullPathToFileOnDisk)
    {
        var fileInfo = new FileInfo(fullPathToFileOnDisk);

        // wait if file is empty (D2 might still be writing)
        if (fileInfo.Length == 0)
        {
            Console.WriteLine("DEBUG: FileInfo length was 0, waiting 1000ms");
            Thread.Sleep(1000);
        }

        using var saveGameStream =
            new FileStream(
                fullPathToFileOnDisk,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);
        var contentFromStream = GetSaveGameContent(saveGameStream);
        return contentFromStream;
    }

    public static DateTime GetLastWriteTime(string fullPathToFileOnDisk)
    {
        var fileInfo = new FileInfo(fullPathToFileOnDisk);
        var lastWriteTime = fileInfo.LastWriteTime;
        return lastWriteTime;
    }

    public byte[] GetSaveGameContent(Stream saveGameFileStream)
    {
        using var tmpMemoryStream = new MemoryStream();
        saveGameFileStream.CopyTo(tmpMemoryStream);
        return tmpMemoryStream.ToArray();
    }
}