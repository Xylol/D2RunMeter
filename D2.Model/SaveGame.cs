using System.Collections;
using System.Reflection;
using System.Text;
using D2.Model.Helper;

namespace D2.Model;

public class SaveGame
{
    private const int widthOfGf = 16;
    private readonly DateTime changedDate;
    private readonly bool[] fileContentBools;
    private readonly bool[] gfPartAsBools;
    private static Dictionary<int, long>? levelExperienceCache;

    public SaveGame(byte[] fileContent, DateTime changedDate)
    {
        this.changedDate = changedDate;
        bool[] fileContentBools = [.. new BitArray(fileContent).Cast<bool>()];
        this.fileContentBools = fileContentBools;
        this.gfPartAsBools = fileContentBools[widthOfGf..];
    }

    public Character GetPlayerCharacter()
    {
        var character = ParseGfValuesFromText(this.gfPartAsBools);
        return character;
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


    public string GetName()
    {
        var nameBits = GetValuesForSingleToken(this.fileContentBools, SaveGameTokens.Name);
        return new string((GetAsciiFromBool(nameBits.Reverse().ToArray())).Reverse().ToArray());
    }

    private Character ParseGfValuesFromText(bool[] input)
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

        var attributeBoolLookup = new Dictionary<string, bool[]>();
        var inputOffset = 0;
        foreach(var lookup in lookups)
        {
            if (inputOffset + defaultIdentifierWidth > input.Length)
            {
                break;
            }

            if (input[inputOffset..(inputOffset+defaultIdentifierWidth)] == ConvertContent.GetBools(lookup.Identifier))
            {
                if(inputOffset + defaultIdentifierWidth + lookup.Length > input.Length)
                {
                    throw new InvalidDataException("Identifier successfully matched but not enough data left to get correct value");
                }

                attributeBoolLookup.Add(lookup.Name, input[(inputOffset + defaultIdentifierWidth)..(inputOffset + defaultIdentifierWidth + lookup.Length)]);
                inputOffset += defaultIdentifierWidth + lookup.Length;
            }
        }

        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(attributeBoolLookup));


        Func<bool[], int> GetIntFromBools = bits => bits.Aggregate(0, (acc, bit) => (acc << 1) | (bit ? 1 : 0));
        Func<bool[], int> GetLongFromBools = bits => bits.Aggregate(0, (acc, bit) => (acc << 1) | (bit ? 1 : 0));
        var level = GetIntFromBools(attributeBoolLookup[SaveGameGfTokens.Level.Name]);
        var experience = GetLongFromBools(attributeBoolLookup[SaveGameGfTokens.Experience.Name]);

        var goldInventory = GetIntFromBools(attributeBoolLookup[SaveGameGfTokens.GoldInventory.Name]);
        var goldStash = GetIntFromBools(attributeBoolLookup[SaveGameGfTokens.GoldStash.Name]);

        var result = new Character
        {
            Name = GetName(),
            LastChangedAt = changedDate,
            Level = level,
            //todo fix long
            Experience = experience, 
            NextLevelAtExperience = GetRequiredExperienceForLevel(level + 1),
            ExperienceRequiredForCurrentLevel = GetRequiredExperienceForLevel(level),
            GoldInventory = goldInventory,
            GoldStash = goldStash 
        };

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
