namespace D2.UI.Services
{
    public class SaveGameHelperService
    {
        public static List<string> GetListOfAvailableCharacters(string? pathToSaveGames)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pathToSaveGames))
                {
                    return [];
                }

                if (!Directory.Exists(pathToSaveGames))
                {
                    return [];
                }

                var directoryInfo = new DirectoryInfo(pathToSaveGames);
                var d2sFiles = directoryInfo.GetFiles("*.d2s");

                var charNames = d2sFiles
                    .OrderByDescending(file => file.LastWriteTime)
                    .Select(file => Path.GetFileNameWithoutExtension(file.Name))
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Cast<string>()
                    .ToList();

                return charNames;
            }
            catch (Exception)
            {
                return [];
            }
        }
    }
}
