using System.Collections;
using D2.Model.Helper;
using NUnit.Framework;
using FluentAssertions;

namespace D2.Model.Tests;

public class ContentConverterTests
{
    [TestFixture]
    public class GetInformations
    {
        private byte[] craftedSaveGame = null!;

        [SetUp]
        public void GetHeaderBytesFromFile()
        {
            var saveGameStream = TestHelper.ResourceStream("D2.Model.Tests.SaveGames.HeaderAndGf.d2s");
            this.craftedSaveGame = new ContentLoader().GetSaveGameContent(saveGameStream);
        }

        [Test]
        public void ReverseEightBitPacks_InputingRawGameBytes_GettingReversedAndRealOrder()
        {
            // Arrange
            var expected = new[]
            {
                false, true, false, true, false, true, false, true, //0101 0101 0x55
                true, false, true, false, true, false, true, false, //1010 1010 0xAA
                false, true, false, true, false, true, false, true, //0101 0101 0x55
                true, false, true, false, true, false, true, false, //1010 1010 0xAA
                false, false, false, false, false, false, false, false, //0000 0000 0x00
                false, true, true, false, false, true, true, true, //0110 0111 0x67
                false, true, true, false, false, true, true, false, //0110 0110 0x66
                false, false, false, false, false, false, false, false //0000 0000 0x00
            };

            // Act
            var actualBitArray = ConvertContent.ReverseEndianess(this.craftedSaveGame);

            // Assert
            actualBitArray.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void GetStringRepresentationFromBitArray_WhenTransforming_AssertsString()
        {
            // Arrange
            var inputBools = new[] {true, true, false};
            var expectedStringRepresentation = "110";

            // Act
            var actualStringRepresentation =
                ConvertContent.GetStringRepresentation(inputBools);

            // Assert
            actualStringRepresentation.Should().BeEquivalentTo(expectedStringRepresentation);
        }

        [Test]
        public void ToBoolArray_WhenProvidedBytes_WillReturnLSB()
        {
            // Arrange
            var testByte = new byte[] {76}; // 76 is 0100 1100 in binary
            var expected = new []
            {
                false, false, true, true,
                false, false, true, false
            };

            // Act
            var actual = ConvertContent.GetBools(testByte);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ToBoolArray_WhenProvidedBitArray_WillReturnLSB()
        {
            // Arrange
            var testByte = new BitArray(new byte[] {76}); // 76 is 0100 1100 in binary
            var expected = new []
            {
                false, false, true, true,
                false, false, true, false
            };

            // Act
            var actual = ConvertContent.GetBools(testByte);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void CreateBoolArrayListWithEightElementsEach_When14Elements_Assert2PacksAndCorrectOrder()
        {
            // Arrange
            var testPack = new []
            {
                true, true, true, true, false, false, false, false,
                false, true, true, false, false, true
            }; // 1111 0000 0110 01
            var expected = new List<bool[]>
            {
                { [true, true, true, true, false, false, false, false] },
                { [false, true, true, false, false, true, false, false] }
            };

            // Act
            var actual = ConvertContent.GetBatchesWithEightElements(testPack);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void CreateBoolArrayListWithEightElementsEach_When16Elements_Assert2PacksAndCorrectOrder()
        {
            // Arrange
            var testPack = new []
            {
                true, true, true, true, false, false, false, false,
                false, true, true, false, false, true, false, true
            }; // 1111 0000 0110 0101
            var expected = new List<bool[]>
            {
                { [true, true, true, true, false, false, false, false] },
                { [false, true, true, false, false, true, false, true] }
            };

            // Act
            var actual = ConvertContent.GetBatchesWithEightElements(testPack);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ReverseBitOrderOfEightElements_When8ElementsProvided_AssertReversed()
        {
            // Arrange
            var testElements = new[] {true, true, false, true, false, true, true, true}; // 1101 0111
            var expected = new[] {true, true, true, false, true, false, true, true}; // 1110 1011

            // Act
            var actual = ConvertContent.ReverseBitOrderOfExactlyEightElements(testElements);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ReverseBitOrderForEachEightElementPack_When2PacksProvided_AssertEachPackReversed()
        {
            // Arrange
            var testPack = new []
            {
                true, true, true, true, false, false, false, false,
                false, true, true, false, false, true, false, true
            }; // 1111 0000 0110 0101
            var expected = new[]
            {
                false, false, false, false, true, true, true, true,
                true, false, true, false, false, true, true, false
            };

            // Act
            var actual = ConvertContent.ReverseBitOrderForEachEightElementPack(testPack);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void WhenWeLoad_SaveGameFromFile_WeAssertTheStringRepresentationOfBits()
        {

            // TODO: verify the true bit sequence as written by the game to the saveGame file.
            // Arrange
            var expectedFirstFiveBytes =
                "0101"+"0101"+ // 0x55
                "1010"+"1010"+ // 0xAA
                "0101"+"0101"+ // 0x55
                "1010"+"1010"+ // 0xAA
                "0000"+"0000"+ // 0x00
                "0110"+"0111"+ // 0x67
                "0110"+"0110"+ // 0x66
                "0000"+"0000"; // 0x00

            // Act
            var actualBools = ConvertContent.GetBools(this.craftedSaveGame).ToArray();
            var actualReversedBools = ConvertContent.ReverseBitOrderForEachEightElementPack(actualBools).ToArray();
            var actualReveresedStringResult = ConvertContent.GetStringRepresentation(actualReversedBools);

            // Assert
            actualReveresedStringResult.Should().BeEquivalentTo(expectedFirstFiveBytes);
        }

        [Test]
        public void GetBoolsFromHex_WhenSingleHexProvided_WeAssertBools()
        {
            // Arrange
            var input = 0x67; // ascii g
            var expected = new[] {false, true, true, false, false, true, true, true}; //0110 0111

            // Act
            var actual = ConvertContent.GetLsbBoolArraysFromByteWideInts([input]);
            var actualConcated = ConvertContent.GetLesserDimensionBoolArray(actual).ToArray();
            var reversedActual = ConvertContent.ReverseBitOrderForEachEightElementPack(actualConcated);

            // Assert
            expected.Should().BeEquivalentTo(reversedActual);
        }

        [Test]
        public void GetBools_WhenWeProvideAValidBitString_WeAssertBooleans()
        {
            // Arrange
            var input = "1011001";
            var expected = new[] {true, false, true, true, false, false, true};

            // Act
            var actual = ConvertContent.GetBools(input);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void GetBools_WhenWeProvideStringWithWrongCharacters_WeAssertArgumentException()
        {
            // Arrange
            var input = "01203102";

            // Act && Assert
            Assert.Throws<ArgumentException>(() => ConvertContent.GetBools(input));
        }

        [Test]
        public void GetNumbers_WhenWeProvide16bits_WeAssert2Numbers()
        {
            // Arrange
            var input = new []
            {
                true, true, true, true, true, true, true, false, // 254
                false, false, false, false, false, true, true, true // 7
            };
            var expected = new [] {254, 7};

            // Act
            var actual = ConvertContent.GetNumbersFromMSB(input);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void GetNumbersFromGfValues_WeProvideBig32BitExamples_AssertNoOverflow()
        {
            // Arrange

            // 11010100 00010011 10101010 11010001

            var testNumberAstLSBText ="11010100000100111010101011010001";
            var testBools = ConvertContent.GetBools(testNumberAstLSBText).ToArray();
            var expected = 2337654827L;

            // Act
            var actual = ConvertContent.GetLongFromLittleEndianBools(testBools);

            // Assert
            actual.Should().Be(expected);
        }

    }
}