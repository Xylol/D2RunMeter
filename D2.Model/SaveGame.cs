using System.Text;
using D2.Model.Helper;

namespace D2.Model
{
    public class SaveGame
    {
        private readonly int[] gfCharactersAsHex = [0x67, 0x66];
        private readonly string gfPartAsText;
        private readonly IParser parser;
        private readonly DateTime changedDate;
        private readonly bool[] reveresedAllBools;
        private readonly Character playerCharacter = new Character();

        public SaveGame(byte[] fileContent, IParser parser, DateTime changedDate)
        {
            this.reveresedAllBools = ConvertContent.ReverseEndianess(fileContent).ToArray();
            this.parser = parser;
            this.changedDate = changedDate;
            this.gfPartAsText = ConvertContent.GetStringRepresentation(
                    ConvertContent.ReverseBitOrderForEachEightElementPack(
                        GetGfBooleans()).ToArray());
        }

        public string GetSubstringStartingWithAsciiGF()
        {
            var gfAsBools = ConvertContent.GetLsbBoolArraysFromByteWideInts(this.gfCharactersAsHex);
            var gfConcatedBools = ConvertContent.GetLesserDimensionBoolArray(gfAsBools).ToArray();
            var gfReversedBools = ConvertContent.ReverseBitOrderForEachEightElementPack(gfConcatedBools).ToArray();
            var gfAsText = ConvertContent.GetStringRepresentation(gfReversedBools);

            var reveresedBoolsAsText = ConvertContent.GetStringRepresentation(this.reveresedAllBools);

            var indexOfGandF = reveresedBoolsAsText.IndexOf(gfAsText);
            var result = reveresedBoolsAsText.Substring(indexOfGandF);

            return result;
        }

        public Character GetPlayerCharacter()
        {
            var parsedText = this.parser.ParseGfValuesFromText(this.gfPartAsText);

            this.playerCharacter.Name = GetName();
            this.playerCharacter.LastChangedAt = this.changedDate;

            foreach (var pair in parsedText)
            {
                var transformedValue =
                    ConvertContent.GetLongFromLittleEndianBools(ConvertContent.GetBools(pair.Value).ToArray());

                switch (pair.Key)
                {
                    case "000000000":
                        this.playerCharacter.Strength = (int) transformedValue;
                        break;
                    case "100000000":
                        this.playerCharacter.Energy = (int) transformedValue;
                        break;
                    case "010000000":
                        this.playerCharacter.Dexterity = (int) transformedValue;
                        break;
                    case "110000000":
                        this.playerCharacter.Vitality = (int) transformedValue;
                        break;
                    case "001000000":
                        this.playerCharacter.StatusLeft = (int) transformedValue;
                        break;
                    case "101000000":
                        this.playerCharacter.SkillLeft = (int) transformedValue;
                        break;
                    case "011000000":
                        this.playerCharacter.Life = (int) transformedValue / 256;
                        break;
                    case "111000000":
                        this.playerCharacter.LifeMax = (int) transformedValue / 256;
                        break;
                    case "000100000":
                        this.playerCharacter.Mana = (int) transformedValue / 256;
                        break;
                    case "100100000":
                        this.playerCharacter.ManaMax = (int) transformedValue / 256;
                        break;
                    case "010100000":
                        this.playerCharacter.Stamina = (int) transformedValue / 256;
                        break;
                    case "110100000":
                        this.playerCharacter.StaminaMax = (int) transformedValue / 256;
                        break;
                    case "001100000":
                        this.playerCharacter.Level = (int) transformedValue;
                        break;
                    case "101100000":
                        this.playerCharacter.Experience = transformedValue;
                        break;
                    case "011100000":
                        this.playerCharacter.GoldInventory = (int) transformedValue;
                        break;
                    case "111100000":
                        this.playerCharacter.GoldStash = (int) transformedValue;
                        break;
                }
            }

            this.playerCharacter.ExperienceRequiredForCurrentLevel = GetRequiredExperienceForLevel(this.playerCharacter.Level);

            var nextLevel = this.playerCharacter.Level + 1;
            this.playerCharacter.NextLevelAtExperience = GetRequiredExperienceForLevel(nextLevel);

            return this.playerCharacter;
        }

        private long GetRequiredExperienceForLevel(int currentLevel)
        {
            const int countOfHeaderRows = 1;
            var linesFromLevelExperienceMapping = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LevelExperienceMapping.ssv"));
            var levelExperienceDictionary = linesFromLevelExperienceMapping.Skip(countOfHeaderRows)
                .Select(line => line.Split(';'))
                .ToDictionary(level => int.Parse(level[0]), experience => long.Parse(experience[1]));
            var minimumExperienceForLevel = levelExperienceDictionary[currentLevel];
            return minimumExperienceForLevel;
        }

        private bool[] GetGfBooleans()
        {
            var gfBitsText = GetSubstringStartingWithAsciiGF();
            var gfBooleans = ConvertContent.GetBools(gfBitsText).ToArray();
            return gfBooleans;
        }

        public string GetName()
        {
            var nameBits = this.parser.GetValuesForSingleToken(this.reveresedAllBools, SaveGameTokens.Name);
            return GetAsciiFromBool(nameBits);
        }

        private string GetAsciiFromBool(bool[] input)
        {
            var nameNumberes = ConvertContent.GetNumbersFromMSB(input);
            var nameBytes = nameNumberes.Select(n => BitConverter.GetBytes(n).First()).ToArray();
            var nameString = Encoding.ASCII.GetString(nameBytes).Trim('\0');
            return nameString ?? throw  new Exception("Name is null");
        }
    }
}