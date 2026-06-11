using System.Reflection;
using System.Text;
using D2.Model.Helper;

namespace D2.Model;

public class SaveGame
{
    private readonly int[] gfCharactersAsHex = [0x67, 0x66];
    private const int widthOfGf = 16;
    private readonly string gfPartAsText;
    private readonly DateTime changedDate;
    private readonly bool[] reversedAllBools;
    private static Dictionary<int, long>? levelExperienceCache;

    public SaveGame(byte[] fileContent, DateTime changedDate)
    {
        this.reversedAllBools = [.. ConvertContent.ReverseEndianess(fileContent)];
        this.changedDate = changedDate;
        this.gfPartAsText = string.Concat(ConvertContent.ReverseBitOrderForEachEightElementPack(GetGfBooleans()).ToBitString().Skip(widthOfGf));
    }

    public string GetSubstringStartingWithAsciiGf()
    {
        var gfAsBools = ConvertContent.GetLsbBoolArraysFromByteWideInts(this.gfCharactersAsHex);
        var gfConcatedBools = ConvertContent.GetLesserDimensionBoolArray(gfAsBools).ToArray();
        var gfReversedBools = ConvertContent.ReverseBitOrderForEachEightElementPack(gfConcatedBools).ToArray();
        var gfAsText = gfReversedBools.ToBitString();

        var reversedBoolsAsText = this.reversedAllBools.ToBitString();

        var indexOfGandF = reversedBoolsAsText.IndexOf(gfAsText, StringComparison.Ordinal);
        var result = reversedBoolsAsText[indexOfGandF..];

        return result;
    }

    public Character GetPlayerCharacter()
    {
        var parsedText = ParseGfValuesFromText(this.gfPartAsText);
        var stats = new Dictionary<string, long>();

        foreach (var pair in parsedText)
        {
            var value = ConvertContent.GetLongFromLittleEndianBools([.. ConvertContent.GetBools(pair.Value)]);
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
        var nameBits = GetValuesForSingleToken(this.reversedAllBools, SaveGameTokens.Name);
        return GetAsciiFromBool(nameBits);
    }

    private static Dictionary<string, string> ParseGfValuesFromText(string inputText)
    {
        const int defaultIdentifierWidth = 9;

        var lookups = new List<(string Identifier, int Length, string Name)>
        {
            (SaveGameGfTokens.Strength.BitFieldIdentifier.ToBitString(), SaveGameGfTokens.Strength.Length, SaveGameGfTokens.Strength.Name),
            (SaveGameGfTokens.Energy.BitFieldIdentifier.ToBitString(), SaveGameGfTokens.Energy.Length, SaveGameGfTokens.Energy.Name),
            (SaveGameGfTokens.Dexterity.BitFieldIdentifier.ToBitString(), SaveGameGfTokens.Dexterity.Length, SaveGameGfTokens.Dexterity.Name),
            (SaveGameGfTokens.Vitality.BitFieldIdentifier.ToBitString(), SaveGameGfTokens.Vitality.Length, SaveGameGfTokens.Vitality.Name),
            (SaveGameGfTokens.StatusLeft.BitFieldIdentifier.ToBitString(), SaveGameGfTokens.StatusLeft.Length, SaveGameGfTokens.StatusLeft.Name),
            (SaveGameGfTokens.SkillLeft.BitFieldIdentifier.ToBitString(), SaveGameGfTokens.SkillLeft.Length, SaveGameGfTokens.SkillLeft.Name),
            (SaveGameGfTokens.Life.BitFieldIdentifier.ToBitString(), SaveGameGfTokens.Life.Length, SaveGameGfTokens.Life.Name),
            (SaveGameGfTokens.LifeMax.BitFieldIdentifier.ToBitString(), SaveGameGfTokens.LifeMax.Length, SaveGameGfTokens.LifeMax.Name),
            (SaveGameGfTokens.Mana.BitFieldIdentifier.ToBitString(), SaveGameGfTokens.Mana.Length, SaveGameGfTokens.Mana.Name),
            (SaveGameGfTokens.ManaMax.BitFieldIdentifier.ToBitString(), SaveGameGfTokens.ManaMax.Length, SaveGameGfTokens.ManaMax.Name),
            (SaveGameGfTokens.Stamina.BitFieldIdentifier.ToBitString(), SaveGameGfTokens.Stamina.Length, SaveGameGfTokens.Stamina.Name),
            (SaveGameGfTokens.StaminaMax.BitFieldIdentifier.ToBitString(), SaveGameGfTokens.StaminaMax.Length, SaveGameGfTokens.StaminaMax.Name),
            (SaveGameGfTokens.Level.BitFieldIdentifier.ToBitString(), SaveGameGfTokens.Level.Length, SaveGameGfTokens.Level.Name),
            (SaveGameGfTokens.Experience.BitFieldIdentifier.ToBitString(), SaveGameGfTokens.Experience.Length, SaveGameGfTokens.Experience.Name),
            (SaveGameGfTokens.GoldInventory.BitFieldIdentifier.ToBitString(), SaveGameGfTokens.GoldInventory.Length, SaveGameGfTokens.GoldInventory.Name),
            (SaveGameGfTokens.GoldStash.BitFieldIdentifier.ToBitString(), SaveGameGfTokens.GoldStash.Length, SaveGameGfTokens.GoldStash.Name)
        };

        var result = new Dictionary<string, string>();
        var inputTextOffset = 0;
        foreach(var lookup in lookups)
        {
            if (inputTextOffset + defaultIdentifierWidth > inputText.Length)
            {
                break;
            }

            if (inputText[inputTextOffset..(inputTextOffset+defaultIdentifierWidth)] == lookup.Identifier)
            {
                if(inputTextOffset + defaultIdentifierWidth + lookup.Length > inputText.Length)
                {
                    throw new InvalidDataException("Identifier successfully matched but not enough data left to get correct value");
                }

                result.Add(lookup.Name, inputText[(inputTextOffset + defaultIdentifierWidth)..(inputTextOffset + defaultIdentifierWidth + lookup.Length)]);
                inputTextOffset += defaultIdentifierWidth + lookup.Length;
            }
        }

        return result;
    }

    private static bool[] GetValuesForSingleToken(bool[] input, ParserToken parserToken) 
        => input[parserToken.Index..(parserToken.Index+parserToken.Length)];

    private static string GetAsciiFromBool(bool[] input)
    {
        var nameNumbers = ConvertContent.GetNumbersFromMSB(input);
        var nameBytes = nameNumbers.Select(n => BitConverter.GetBytes(n).First()).ToArray();
        var nameString = Encoding.ASCII.GetString(nameBytes).Trim('\0');
        return nameString ?? throw new Exception("Name is null");
    }
}
