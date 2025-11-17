using System;
using System.Threading;

namespace D2.Model
{
    public class CharacterDataLoader
    {
        private readonly ContentLoader contentLoader;
        private readonly Parser parser;
        private readonly string characterLocation;
        private int countOfChangedDataAfterRead;
        private const int MaximumRetriesAfterChangedData = 10;

        public CharacterDataLoader(string characterLocation)
        {
            this.countOfChangedDataAfterRead = 0;
            this.contentLoader = new ContentLoader();
            this.parser = new Parser();
            this.characterLocation = characterLocation;
        }

        public Character GetCurrentCharacterData()
        {
            var changedDate = this.contentLoader.GetLastWriteTime(this.characterLocation);
            var fileContent = this.contentLoader.GetSaveGameContent(this.characterLocation);
            var changedDateAfterLoading = this.contentLoader.GetLastWriteTime(this.characterLocation);

            if (changedDate == changedDateAfterLoading)
            {
                this.countOfChangedDataAfterRead = 0;
                return new SaveGame(fileContent, this.parser, changedDate).GetPlayerCharacter();
            }

            this.countOfChangedDataAfterRead++;

            if (this.countOfChangedDataAfterRead < MaximumRetriesAfterChangedData)
            {
                Thread.Sleep(100);
                GetCurrentCharacterData();
            }
            throw new Exception("Character was changed to fast between loads. This should not happen.");
        }
    }
}