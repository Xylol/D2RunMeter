using D2.Model.Helper;

namespace D2.Model
{
    public record ParserToken(int Index, int Length, string Name, IEnumerable<bool> BitFieldIdentifier)
    {
        public ParserToken(int index, int length, string name, string bitFieldIdentifierText)
            : this(index, length, name, ConvertContent.GetBools(bitFieldIdentifierText))
        {
        }
    }
}