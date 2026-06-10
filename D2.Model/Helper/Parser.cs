namespace D2.Model.Helper;

public static class Parser
{
    public static bool[] GetValuesForSingleToken(bool[] input, ParserToken parserToken) 
        => input[parserToken.Index..(parserToken.Index+parserToken.Length)];

    public static Dictionary<string, string> ParseGfValuesFromText(string inputText)
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
            if (inputTextOffset+defaultIdentifierWidth > inputText.Length)
            {
                break;
            }

            if (inputText[inputTextOffset..(inputTextOffset+defaultIdentifierWidth)] == lookup.Identifier)
            {
                if(inputTextOffset+defaultIdentifierWidth+lookup.Length > inputText.Length)
                {
                    throw new InvalidDataException("Identifier successfully matched but not enough data left to get correct value");
                }

                result.Add(lookup.Name, inputText[(inputTextOffset+defaultIdentifierWidth)..(inputTextOffset+defaultIdentifierWidth+lookup.Length)]);
                inputTextOffset += defaultIdentifierWidth + lookup.Length;
            }
        }

        return result;
    }
}
