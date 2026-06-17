using System.Collections;
using System.Reflection;
using System.Text;
using D2.Model.Helper;

namespace D2.Model;

public class SaveGame
{
    private readonly (int g, int f) gfCharactersAsHex = (g: 0x67, f: 0x66);
    private const int WidthOfGf = 2;
    private const int BitsPerByte = 8;
    private readonly DateTime changedDate;
    private readonly bool[] fileContentBools;
    private readonly bool[] gfPartAsBools;
    private static Dictionary<int, long>? levelExperienceCache;

    public SaveGame(byte[] fileContent, DateTime changedDate)
    {
        this.changedDate = changedDate;
        bool[] contentBools = [.. new BitArray(fileContent).Cast<bool>()];
        this.fileContentBools = contentBools;

        var startOfGfSection = GetPositionOfGfCharactersFromSavegame(fileContent);
        this.gfPartAsBools = contentBools[((startOfGfSection + WidthOfGf) * BitsPerByte) ..];
    }

    public Character GetPlayerCharacter()
    {
        var character = ParseGfValues(this.gfPartAsBools);
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

        using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new Exception($"Embedded resource '{resourceName}' not found");
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
        return new string(GetAsciiFromBool(nameBits));
    }

    private int GetPositionOfGfCharactersFromSavegame(byte[] savegameBytes)
    {
        for (var i = 0; i < savegameBytes.Length - 1; i++)
        {
            var current = savegameBytes[i];
            var next = savegameBytes[i+1];
            if (current == gfCharactersAsHex.g && next == gfCharactersAsHex.f)
            {
                return i;
            }

        }
        throw new Exception("Malformed Savegame, no gf");
    }

    private Character ParseGfValues(bool[] input)
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

            var inputIdentifierPart = input[inputOffset..(inputOffset + defaultIdentifierWidth)];
            if (inputIdentifierPart.SequenceEqual(ConvertContent.GetBools(lookup.Identifier)))
            {
                if(inputOffset + defaultIdentifierWidth + lookup.Length > input.Length)
                {
                    throw new InvalidDataException("Identifier successfully matched but not enough data left to get correct value");
                }

                attributeBoolLookup.Add(lookup.Name, input[(inputOffset + defaultIdentifierWidth)..(inputOffset + defaultIdentifierWidth + lookup.Length)]);
                inputOffset += defaultIdentifierWidth + lookup.Length;
            }
        }

        // stats with value 0 are not in stream so if something missing set 0
        long ReadValue(string name) => attributeBoolLookup.TryGetValue(name, out var bits)
            ? ConvertContent.GetLongFromLittleEndianBools(bits)
            : 0L;

        var level = (int) ReadValue(SaveGameGfTokens.Level.Name);
        var experience = ReadValue(SaveGameGfTokens.Experience.Name);
        var goldInventory = (int) ReadValue(SaveGameGfTokens.GoldInventory.Name);
        var goldStash = (int) ReadValue(SaveGameGfTokens.GoldStash.Name);

        var result = new Character
        {
            Name = GetName(),
            LastChangedAt = changedDate,
            Level = level,
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
        var bits = new BitArray(input);
        // do not cut bytes
        var nameBytes = new byte[(int)Math.Ceiling(input.Length/8.0)];
        bits.CopyTo(nameBytes, 0);
        
        var nameString = Encoding.ASCII.GetString(nameBytes).Trim('\0');
        return nameString ?? throw new Exception("Name is null");
    }
}
