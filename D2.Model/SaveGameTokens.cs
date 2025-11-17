namespace D2.Model;

public static class SaveGameTokens
{
    // TODO: read from the mpq instead maybe in the ItemStatsCost.txt file
    private const int NumberOfBitsInAByte = 8;

    public static readonly ParserToken Name = new ParserToken(160, 16 * NumberOfBitsInAByte, "Name", "0");
}