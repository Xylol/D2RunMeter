using System.Collections;

namespace D2.Model.Helper;

public static class ConvertContent
{
    private const int BitsPerByte = 8;

    public static IEnumerable<bool> GetBools(byte[] inputBytes) => new BitArray(inputBytes).Cast<bool>();

    public static IEnumerable<bool> GetBools(string inputText)
    {
        if (string.IsNullOrEmpty(inputText) || inputText.Any(b => b is not ('0' or '1')))
        {
            throw new ArgumentException($"InputText must contain only 0 or 1 but was '{inputText}'", inputText);
        }

        var result = new List<bool>();
        result.AddRange(inputText.Select(b => b.Equals('1')));

        return result;
    }

    public static IEnumerable<int> GetNumbersFromMSB(bool[] input)
    {
        var result = new List<int>();
        var booleanBatches = GetBatchesWithEightElements(input);

        var singleNumber = 0;
        foreach (var batch in booleanBatches)
        {
            for (var i = 0; i < BitsPerByte; i++)
            {
                singleNumber += batch[BitsPerByte - 1 - i] ? (int) Math.Pow(2, i) : 0;
            }
            result.Add(singleNumber);
            singleNumber = 0;
        }

        return result;
    }

    public static long GetLongFromLittleEndianBools(bool[] input)
    {
        var result = 0L;
        for (var i = 0; i < input.Length; i++)
        {
            result += input[i] ? (long) Math.Pow(2, i) : 0L;
        }

        return result;
    }

    public static IEnumerable<bool[]> GetBatchesWithEightElements(bool[] inputElements)
    {
        if (inputElements.Length == 0)
        {
            throw new ArgumentException("Array empty - check this.", nameof(inputElements));
        }

        var localElements = inputElements.ToList();

        while (localElements.Count % BitsPerByte != 0)
        {
            localElements.Add(false);
        }

        return localElements.Chunk(BitsPerByte);
    }
}
