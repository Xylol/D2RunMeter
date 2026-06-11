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
        //  TODO: fix larger than 8 bit calculations this might be related to a few bugs.
        //  Calculation of SaveGames where differing from PK and where byte shifts are needed might be related to this.
        //if (input.Length == 0 || input.Length > 8)
        //{
        //    throw new Exception("Input array should not be empty or longer than 8 elements.");
        //}

        var result = 0L;
        for (var i = 0; i < input.Length; i++)
        {
            result += input[i] ? (long) Math.Pow(2, i) : 0L;
        }

        return result;
    }

    public static IEnumerable<bool[]> GetLsbBoolArraysFromByteWideInts(int[] input)
    {
        var result = new List<bool[]>();
        foreach (var single in input)
        {
            result.Add([.. GetBools([Convert.ToByte(single)])]);
        }
        return result;
    }

    public static IEnumerable<bool> GetLesserDimensionBoolArray(IEnumerable<bool[]> inputBoolCollection)
    {
        var result = new List<bool>();
        foreach (var singleArray in inputBoolCollection)
        {
            result.AddRange(singleArray);
        }

        return result;
    }

    public static IEnumerable<bool> ReverseEndianess(byte[] inputOrder)
    {
        return ReverseBitOrderForEachEightElementPack(GetBools(inputOrder).ToArray());
    }

    public static IEnumerable<bool> ReverseBitOrderForEachEightElementPack(bool[] originalOrderBools)
    {
        ArgumentNullException.ThrowIfNull(originalOrderBools);

        if (originalOrderBools.Length % BitsPerByte != 0)
        {
            throw new Exception($"The provided input is not dividable by eight, its length is {originalOrderBools.Length}.");
        }

        var batchesWithEightElements = GetBatchesWithEightElements(originalOrderBools);
        var reversedOrderBoolsResult = new List<bool>();

        foreach (var batch in batchesWithEightElements)
        {
            var eightReversedOrderBools = ReverseBitOrderOfExactlyEightElements(batch);
            reversedOrderBoolsResult.AddRange(eightReversedOrderBools);
        }

        return reversedOrderBoolsResult;
    }

    public static IEnumerable<bool> ReverseBitOrderOfExactlyEightElements(bool[] originalOrderInput) => originalOrderInput.Reverse();

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
