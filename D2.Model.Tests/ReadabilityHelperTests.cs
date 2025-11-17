using NUnit.Framework;
using FluentAssertions;

namespace D2.Model.Tests
{
    [TestFixture]
    public class ReadabilityHelperTests
    {
        // TODO: consider culture invariant
        [TestCase(0, "0")]
        [TestCase(1,"1")]
        [TestCase(50498,"50498")]
        [TestCase(50499,"50499")]
        [TestCase(50500,"51k")]
        [TestCase(999498,"999k")]
        [TestCase(999499,"999k")]
        [TestCase(999500,"1.00M")]
        [TestCase(1005000,"1.01M")]
        [TestCase(5099999,"5.10M")]
        [TestCase(5100000,"5.1M")]
        [TestCase(36376555,"36.4M")]
        [TestCase(363765550,"363.8M")]
        [TestCase(999499999,"999.5M")]
        [TestCase(999500000,"1.00G")]
        [TestCase(999999999,"1.00G")]
        [TestCase(1000000000,"1.00G")]
        [TestCase(3520485254,"3.52G")]
        [TestCase(-0, "0")]
        [TestCase(-1,"-1")]
        [TestCase(-50498,"-50498")]
        [TestCase(-50499,"-50499")]
        [TestCase(-50500,"-51k")]
        [TestCase(-999498,"-999k")]
        [TestCase(-999499,"-999k")]
        [TestCase(-999500,"-1.00M")]
        [TestCase(-1005000,"-1.01M")]
        [TestCase(-5099999,"-5.10M")]
        [TestCase(-5100000,"-5.1M")]
        [TestCase(-36376555,"-36.4M")]
        [TestCase(-363765550,"-363.8M")]
        [TestCase(-999499999,"-999.5M")]
        [TestCase(-999500000,"-1.00G")]
        [TestCase(-999999999,"-1.00G")]
        [TestCase(-1000000000,"-1.00G")]
        [TestCase(-3520485254,"-3.52G")]
        public void ConvertToSI_When_edgeCasesProvided_Should_returnRightUnitAndDecimals(double inputValue, string expected)
        {
            var actual = ReadabilityHelper.ConvertToSi(inputValue);
            actual.Should().Be(expected);
        }

        [TestCase(0, "0m")]
        [TestCase(0.0, "0m")]
        [TestCase(0.99,"59m")]
        [TestCase(1,"01h 00m")]
        [TestCase(1.0,"01h 00m")]
        [TestCase(1.01,"01h 00m")]
        [TestCase(1.02,"01h 01m")]
        [TestCase(72.45,"72h 27m")]
        public void ConvertToHoursAndMinutesText_When_inputLargerOneAndSmaller_Should_returnWithHoursandWithout(double input, string expected)
        {
            var actual = ReadabilityHelper.ConvertToHoursAndMinutesText(input);
            actual.Should().Be(expected);
        }
    }
}