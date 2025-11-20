using System.Reflection;
using System.Text;
using D2.Model.Helper;

namespace D2.Model;

public class SaveGame
{
    private readonly int[] gfCharactersAsHex = [0x67, 0x66];
    private readonly string gfPartAsText;
    private readonly DateTime changedDate;
    private readonly bool[] reversedAllBools;
    private static Dictionary<int, long>? levelExperienceCache;

    public SaveGame(byte[] fileContent, DateTime changedDate)
    {
        this.reversedAllBools = ConvertContent.ReverseEndianess(fileContent).ToArray();
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

        var reversedBoolsAsText = ConvertContent.GetStringRepresentation(this.reversedAllBools);

        var indexOfGandF = reversedBoolsAsText.IndexOf(gfAsText, StringComparison.Ordinal);
        var result = reversedBoolsAsText.Substring(indexOfGandF);

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
        var level = (int)stats.GetValueOrDefault(SaveGameGfTokens.Level.Name, 1);

        return new Character
        {
            Name = GetName(),
            LastChangedAt = changedDate,
            Level = level,
            Experience = stats.GetValueOrDefault(SaveGameGfTokens.Experience.Name, 0),
            NextLevelAtExperience = GetRequiredExperienceForLevel(level + 1),
            ExperienceRequiredForCurrentLevel = GetRequiredExperienceForLevel(level),
            GoldInventory = (int)stats.GetValueOrDefault(SaveGameGfTokens.GoldInventory.Name, 0),
            GoldStash = (int)stats.GetValueOrDefault(SaveGameGfTokens.GoldStash.Name, 0)
        };
    }

    private static long GetRequiredExperienceForLevel(int currentLevel)
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
        const string resourceName = "D2.Model.LevelExperienceMapping.ssv";

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
        var nameBits = Parser.GetValuesForSingleToken(this.reversedAllBools, SaveGameTokens.Name);
        return GetAsciiFromBool(nameBits);
    }

    private static string GetAsciiFromBool(bool[] input)
    {
        var nameNumbers = ConvertContent.GetNumbersFromMSB(input);
        var nameBytes = nameNumbers.Select(n => BitConverter.GetBytes(n).First()).ToArray();
        var nameString = Encoding.ASCII.GetString(nameBytes).Trim('\0');
        return nameString ?? throw new Exception("Name is null");
    }
}