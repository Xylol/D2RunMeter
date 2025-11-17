using System.Text;
using D2.Model.Helper;

namespace D2.Model;

public class SaveGame
{
    private readonly int[] gfCharactersAsHex = [0x67, 0x66];
    private readonly string gfPartAsText;
    private readonly DateTime changedDate;
    private readonly bool[] reveresedAllBools;

    public SaveGame(byte[] fileContent, DateTime changedDate)
    {
        this.reveresedAllBools = ConvertContent.ReverseEndianess(fileContent).ToArray();
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
        var parsedText = Parser.ParseGfValuesFromText(this.gfPartAsText);

        var characterName = GetName();
        var characterStrength = 0;
        var characterEnergy = 0;
        var characterDexterity = 0;
        var characterVitality = 0;
        var characterStatusLeft = 0;
        var characterSkillLeft = 0;
        var characterLife = 0;
        var characterLifeMax = 0;
        var characterMana = 0;
        var characterManaMax = 0;
        var characterStamina = 0;
        var characterStaminaMax = 0;
        var characterLevel = 0;
        var characterExperience = 0L;
        var characterGoldInventory = 0;
        var characterGoldStash = 0;

        foreach (var pair in parsedText)
        {
            var transformedValue =
                ConvertContent.GetLongFromLittleEndianBools(ConvertContent.GetBools(pair.Value).ToArray());

            switch (pair.Key)
            {
                case "000000000":
                    characterStrength = (int)transformedValue;
                    break;
                case "100000000":
                    characterEnergy = (int)transformedValue;
                    break;
                case "010000000":
                    characterDexterity = (int)transformedValue;
                    break;
                case "110000000":
                    characterVitality = (int)transformedValue;
                    break;
                case "001000000":
                    characterStatusLeft = (int)transformedValue;
                    break;
                case "101000000":
                    characterSkillLeft = (int)transformedValue;
                    break;
                case "011000000":
                    characterLife = (int)transformedValue / 256;
                    break;
                case "111000000":
                    characterLifeMax = (int)transformedValue / 256;
                    break;
                case "000100000":
                    characterMana = (int)transformedValue / 256;
                    break;
                case "100100000":
                    characterManaMax = (int)transformedValue / 256;
                    break;
                case "010100000":
                    characterStamina = (int)transformedValue / 256;
                    break;
                case "110100000":
                    characterStaminaMax = (int)transformedValue / 256;
                    break;
                case "001100000":
                    characterLevel = (int)transformedValue;
                    break;
                case "101100000":
                    characterExperience = transformedValue;
                    break;
                case "011100000":
                    characterGoldInventory = (int)transformedValue;
                    break;
                case "111100000":
                    characterGoldStash = (int)transformedValue;
                    break;
            }
        }

        var characterExperienceRequiredForCurrentLevel = GetRequiredExperienceForLevel(characterLevel);

        var nextLevel = characterLevel + 1;
        var characterNextLevelAtExperience = GetRequiredExperienceForLevel(nextLevel);

        var character = new Character
        {
            Name = characterName,
            LastChangedAt = changedDate,
            Level = characterLevel,
            Experience = characterExperience,
            NextLevelAtExperience = characterNextLevelAtExperience,
            ExperienceRequiredForCurrentLevel = characterExperienceRequiredForCurrentLevel,
            GoldInventory = characterGoldInventory,
            GoldStash = characterGoldStash,
            Strength = characterStrength,
            Dexterity = characterDexterity,
            Vitality = characterVitality,
            Energy = characterEnergy,
            StatusLeft = characterStatusLeft,
            Life = characterLife,
            LifeMax = characterLifeMax,
            Stamina = characterStamina,
            StaminaMax = characterStaminaMax,
            Mana = characterMana,
            ManaMax = characterManaMax,
            SkillLeft = characterSkillLeft 
        };
        return character;
    }

    private long GetRequiredExperienceForLevel(int currentLevel)
    {
        const int countOfHeaderRows = 1;
        var linesFromLevelExperienceMapping = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "LevelExperienceMapping.ssv"));
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
        var nameBits = Parser.GetValuesForSingleToken(this.reveresedAllBools, SaveGameTokens.Name);
        return GetAsciiFromBool(nameBits);
    }

    private string GetAsciiFromBool(bool[] input)
    {
        var nameNumberes = ConvertContent.GetNumbersFromMSB(input);
        var nameBytes = nameNumberes.Select(n => BitConverter.GetBytes(n).First()).ToArray();
        var nameString = Encoding.ASCII.GetString(nameBytes).Trim('\0');
        return nameString ?? throw new Exception("Name is null");
    }
}