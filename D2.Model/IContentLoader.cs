using System.IO;

namespace D2.Model
{
    public interface IContentLoader
    {
        byte[] GetSaveGameContent(string fullPathToFileOnDisk);
        byte[] GetSaveGameContent(Stream saveGameFileStream);
    }
}