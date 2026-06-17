public static class BoolEnumerableExtensions
{
    public static string ToBitString(this IEnumerable<bool> inputBools)
    {
        var enumeratedInput = inputBools as bool[] ?? inputBools.ToArray();
        if (enumeratedInput.Length == 0)
        {
            throw new ArgumentException("Input was empty.");
        }

        return string.Concat(enumeratedInput.Select(b => b.Equals(true) ? "1" : "0"));
    }
}
