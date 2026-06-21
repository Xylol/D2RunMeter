namespace D2.Model;

public class CharacterDataLoader(string characterLocation, IContentLoader contentLoader)
{
    private const int MaximumLoadingAttempts = 10;

    public string CharacterFilePath => characterLocation;

    public DateTime GetLastWriteTime()
    {
        return ContentLoader.GetLastWriteTime(characterLocation);
    }

    public Character GetCurrentCharacterData()
    {
        var attemptCounter = 1;
        while (true)
        {
            var fileLastUpdatedAt = ContentLoader.GetLastWriteTime(characterLocation);
            var fileContent = contentLoader.GetSaveGameContent(characterLocation);

            if (fileLastUpdatedAt == ContentLoader.GetLastWriteTime(characterLocation))
            {
                return new SaveGame(fileContent, fileLastUpdatedAt).GetPlayerCharacter();
            }

            if (attemptCounter >= MaximumLoadingAttempts)
            {
                throw new IOException($"Character file '{characterLocation}' kept changing across {MaximumLoadingAttempts} read attempts");
            }

            attemptCounter++;
            Thread.Sleep(100);
        }
    }
}
