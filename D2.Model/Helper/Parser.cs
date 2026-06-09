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
            ("000000000", 10, SaveGameGfTokens.Strength.Name),
            ("100000000", 10, SaveGameGfTokens.Energy.Name),
            ("010000000", 10, SaveGameGfTokens.Dexterity.Name),
            ("110000000", 10, SaveGameGfTokens.Vitality.Name),
            ("001000000", 10, SaveGameGfTokens.StatusLeft.Name),
            ("101000000", 08, SaveGameGfTokens.SkillLeft.Name),
            ("011000000", 21, SaveGameGfTokens.Life.Name),
            ("111000000", 21, SaveGameGfTokens.LifeMax.Name),
            ("000100000", 21, SaveGameGfTokens.Mana.Name),
            ("100100000", 21, SaveGameGfTokens.ManaMax.Name),
            ("010100000", 21, SaveGameGfTokens.Stamina.Name),
            ("110100000", 21, SaveGameGfTokens.StaminaMax.Name),
            ("001100000", 07, SaveGameGfTokens.Level.Name),
            ("101100000", 32, SaveGameGfTokens.Experience.Name),
            ("011100000", 25, SaveGameGfTokens.GoldInventory.Name),
            ("111100000", 25, SaveGameGfTokens.GoldStash.Name)
        };

        var result = new Dictionary<string, string>();
        var inputTextOffset = 0;
        foreach( var lookup in lookups)
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
