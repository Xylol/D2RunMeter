using System.Collections.Generic;

namespace D2.Model
{
    public interface IParser
    {
        bool[] GetValuesForSingleToken(bool[] input, ParserToken parserToken);
        Dictionary<string, string> ParseGfValuesFromText(string inputText);
    }
}