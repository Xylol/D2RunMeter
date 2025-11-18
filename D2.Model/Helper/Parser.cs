namespace D2.Model.Helper;

public static class Parser 
{
    public static bool[] GetValuesForSingleToken(bool[] input, ParserToken parserToken)
    {
        var resultLookup = GetParsedValuesFrom(input, [parserToken]);
        var result = resultLookup.Values.Single();
        return result;
    }

    public static Dictionary<string, bool[]> GetParsedValuesFrom(bool[] input, ParserToken[] parserTokens)
    {
        // TODO: use json instead of dictionary?

        var resultValues = new Dictionary<string, bool[]>();

        foreach (var token in parserTokens)
        {
            var resultForSingleToken = new List<bool>();

            for (var i = token.Index; i < token.Index + token.Length; i++)
            {
                resultForSingleToken.Add(input[i]);
            }

            resultValues[token.Name] = resultForSingleToken.ToArray();
        }

        return resultValues;
    }

    public static Dictionary<string, string> ParseGfValuesFromText(string inputText)
    {
        const int widthOfGf = 16;
        const int defaultIdentifierWidth = 9;

        var lookups = new List<Tuple<string, int, string>>
        {
            new ("000000000", 10, SaveGameGfTokens.Strength.Name),
            new ("100000000", 10, SaveGameGfTokens.Energy.Name),
            new ("010000000", 10, SaveGameGfTokens.Dexterity.Name),
            new ("110000000", 10, SaveGameGfTokens.Vitality.Name),
            new ("001000000", 10, SaveGameGfTokens.StatusLeft.Name),
            new ("101000000", 8, SaveGameGfTokens.SkillLeft.Name),
            new ("011000000", 21, SaveGameGfTokens.Life.Name),
            new ("111000000", 21, SaveGameGfTokens.LifeMax.Name),
            new ("000100000", 21, SaveGameGfTokens.Mana.Name),
            new ("100100000", 21, SaveGameGfTokens.ManaMax.Name),
            new ("010100000", 21, SaveGameGfTokens.Stamina.Name),
            new ("110100000", 21, SaveGameGfTokens.StaminaMax.Name),
            new ("001100000", 7, SaveGameGfTokens.Level.Name),
            new ("101100000", 32,SaveGameGfTokens.Experience.Name),
            new ("011100000", 25, SaveGameGfTokens.GoldInventory.Name),
            new ("111100000", 25, SaveGameGfTokens.GoldStash.Name)
        };

        var result = new Dictionary<string, string>();

        for (var i = widthOfGf; i < inputText.Length && lookups.Any();)
        {
            var checkedText = string.Concat(inputText.Skip(i).Take(defaultIdentifierWidth));
            if (checkedText.Equals(lookups.First().Item1))
            {
                result.Add(lookups.First().Item3, string.Concat(inputText.Skip(defaultIdentifierWidth + i).Take(lookups.First().Item2)));
                i += defaultIdentifierWidth + lookups.First().Item2;
            }

            lookups.RemoveAt(0);
        }

        return result;
    }
}