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
        public void GetStringRepresentationFromBitArray_WhenTransforming_AssertsString()
        {
            // Arrange
            var inputBools = new[] {true, true, false};
            var expectedStringRepresentation = "110";

            // Act
            var actualStringRepresentation = inputBools.ToBitString();

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
