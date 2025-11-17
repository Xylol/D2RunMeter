namespace D2.Model;

public class CharacterDataLoader(string characterLocation, IContentLoader contentLoader)
{
    private const int MaximumLoadingAttempts = 10;

    public DateTime GetLastWriteTime()
    {
        return ContentLoader.GetLastWriteTime(characterLocation);
    }

    public Character GetCurrentCharacterData()
    {
        var attemptCounter = 0;
        while (true)
        {
            attemptCounter++;
                
            var updatedAt = ContentLoader.GetLastWriteTime(characterLocation);
            var fileContent = contentLoader.GetSaveGameContent(characterLocation);
            var updatedAtAfterLoadingFileContent = ContentLoader.GetLastWriteTime(characterLocation);

            if (updatedAt == updatedAtAfterLoadingFileContent)
            {
                return new SaveGame(fileContent, updatedAt).GetPlayerCharacter();
            }

            if (attemptCounter < MaximumLoadingAttempts)
            {
                Thread.Sleep(100);
                continue;
            }

            throw new Exception("Character was changed too fast between loads. This should not happen.");
        }
    }
}