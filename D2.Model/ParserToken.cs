using System.Collections.Generic;
using D2.Model.Helper;

namespace D2.Model
{
    public class ParserToken
    {
        public int Index { get; }
        public int Length { get; }
        public string Name { get; }
        private IEnumerable<bool> BitFieldIdentifier { get; }

        public ParserToken(int index, int length, string name, IEnumerable<bool> bitFieldIdentifier)
        {
            this.Index = index;
            this.Length = length;
            this.Name = name;
            this.BitFieldIdentifier = bitFieldIdentifier;
        }

        public ParserToken(int index, int length, string name, string bitFieldIdentifierText)
            : this(index, length, name, ConvertContent.GetBools(bitFieldIdentifierText))
        {
        }
    }
}