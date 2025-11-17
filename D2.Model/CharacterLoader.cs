namespace D2.Model
{
    public class CharacterDataLoader(string characterLocation, IContentLoader contentLoader, IParser parser)
    {
        private const int MaximumLoadingAttempts = 10;

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
                    return new SaveGame(fileContent, parser, updatedAt).GetPlayerCharacter();
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
}