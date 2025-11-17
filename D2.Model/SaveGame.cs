using System.Reflection;
using System.Text;
using D2.Model.Helper;

namespace D2.Model;

public class SaveGame
{
    private readonly int[] gfCharactersAsHex = [0x67, 0x66];
    private readonly string gfPartAsText;
    private readonly DateTime changedDate;
    private readonly bool[] reveresedAllBools;
    private static Dictionary<int, long>? levelExperienceCache;

    public SaveGame(byte[] fileContent, DateTime changedDate)
    {
        this.reveresedAllBools = ConvertContent.ReverseEndianess(fileContent).ToArray();
        this.changedDate = changedDate;
        this.gfPartAsText = ConvertContent.GetStringRepresentation(
            ConvertContent.ReverseBitOrderForEachEightElementPack(
                GetGfBooleans()).ToArray());
    }

    public string GetSubstringStartingWithAsciiGf()
    {
        var gfAsBools = ConvertContent.GetLsbBoolArraysFromByteWideInts(this.gfCharactersAsHex);
        var gfConcatedBools = ConvertContent.GetLesserDimensionBoolArray(gfAsBools).ToArray();
        var gfReversedBools = ConvertContent.ReverseBitOrderForEachEightElementPack(gfConcatedBools).ToArray();
        var gfAsText = ConvertContent.GetStringRepresentation(gfReversedBools);

        var reveresedBoolsAsText = ConvertContent.GetStringRepresentation(this.reveresedAllBools);

        var indexOfGandF = reveresedBoolsAsText.IndexOf(gfAsText, StringComparison.Ordinal);
        var result = reveresedBoolsAsText.Substring(indexOfGandF);

        return result;
    }

    public Character GetPlayerCharacter()
    {
        var parsedText = Parser.ParseGfValuesFromText(this.gfPartAsText);
        var stats = new Dictionary<string, long>();

        foreach (var pair in parsedText)
        {
            var value = ConvertContent.GetLongFromLittleEndianBools(
                ConvertContent.GetBools(pair.Value).ToArray());
            stats[pair.Key] = value;
        }
        var level = (int)stats.GetValueOrDefault("001100000", 0);

        return new Character
        {
            Name = GetName(),
            LastChangedAt = changedDate,
            Level = level,
            Experience = stats.GetValueOrDefault("101100000", 0),
            NextLevelAtExperience = GetRequiredExperienceForLevel(level + 1),
            ExperienceRequiredForCurrentLevel = GetRequiredExperienceForLevel(level),
            Strength = (int)stats.GetValueOrDefault("000000000", 0),
            Energy = (int)stats.GetValueOrDefault("100000000", 0),
            Dexterity = (int)stats.GetValueOrDefault("010000000", 0),
            Vitality = (int)stats.GetValueOrDefault("110000000", 0),
            StatusLeft = (int)stats.GetValueOrDefault("001000000", 0),
            SkillLeft = (int)stats.GetValueOrDefault("101000000", 0),
            Life = (int)stats.GetValueOrDefault("011000000", 0) / 256,
            LifeMax = (int)stats.GetValueOrDefault("111000000", 0) / 256,
            Mana = (int)stats.GetValueOrDefault("000100000", 0) / 256,
            ManaMax = (int)stats.GetValueOrDefault("100100000", 0) / 256,
            Stamina = (int)stats.GetValueOrDefault("010100000", 0) / 256,
            StaminaMax = (int)stats.GetValueOrDefault("110100000", 0) / 256,
            GoldInventory = (int)stats.GetValueOrDefault("011100000", 0),
            GoldStash = (int)stats.GetValueOrDefault("111100000", 0)
        };
    }

    private long GetRequiredExperienceForLevel(int currentLevel)
    {
        if (levelExperienceCache == null)
        {
            LoadLevelExperienceMappingFromEmbeddedResource();
        }

        return levelExperienceCache![currentLevel];
    }

    private static void LoadLevelExperienceMappingFromEmbeddedResource()
    {
        const int countOfHeaderRows = 1;
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "D2.Model.LevelExperienceMapping.ssv";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new Exception($"Embedded resource '{resourceName}' not found");
        using var reader = new StreamReader(stream);

        var lines = new List<string>();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line != null)
            {
                lines.Add(line);
            }
        }

        levelExperienceCache = lines.Skip(countOfHeaderRows)
            .Select(line => line.Split(';'))
            .ToDictionary(level => int.Parse(level[0]), experience => long.Parse(experience[1]));
    }

    private bool[] GetGfBooleans()
    {
        var gfBitsText = GetSubstringStartingWithAsciiGf();
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