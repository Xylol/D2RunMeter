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

        var lookups = new List<Tuple<string, int>>
        {
            new Tuple<string, int>("000000000", 10),
            new Tuple<string, int>("100000000", 10),
            new Tuple<string, int>("010000000", 10),
            new Tuple<string, int>("110000000", 10),
            new Tuple<string, int>("001000000", 10),
            new Tuple<string, int>("101000000", 8),
            new Tuple<string, int>("011000000", 21),
            new Tuple<string, int>("111000000", 21),
            new Tuple<string, int>("000100000", 21),
            new Tuple<string, int>("100100000", 21),
            new Tuple<string, int>("010100000", 21),
            new Tuple<string, int>("110100000", 21),
            new Tuple<string, int>("001100000", 7),
            new Tuple<string, int>("101100000", 32),
            new Tuple<string, int>("011100000", 25),
            new Tuple<string, int>("111100000", 25)
        };

        var result = new Dictionary<string, string>();

        for (var i = widthOfGf; i < inputText.Length && lookups.Any();)
        {
            var checkedText = string.Concat(inputText.Skip(i).Take(defaultIdentifierWidth));
            if (checkedText.Equals(lookups.First().Item1))
            {
                result.Add(lookups.First().Item1, string.Concat(inputText.Skip(defaultIdentifierWidth + i).Take(lookups.First().Item2)));
                i += defaultIdentifierWidth + lookups.First().Item2;
            }

            lookups.RemoveAt(0);
        }

        return result;
    }
}