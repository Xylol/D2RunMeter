public static class BoolEnumerableExtensions
{
    public static string ToBitString(this IEnumerable<bool> inputBools)
    {
        if (!inputBools.Any())
        {
            throw new ArgumentException("Input was empty.");
        }

        return string.Concat(inputBools.Select(b => b.Equals(true) ? "1" : "0"));
    }
}
