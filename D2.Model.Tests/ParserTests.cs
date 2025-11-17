using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace D2.Model.Tests
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void ParserToken_WhenCreated_WeCanAssertName()
        {
            // Arrange
            var expected = "strengthToken";
            var strengthToken = new ParserToken(0, 0, expected, "0");

            // Act
            var actual = strengthToken.Name;

            // Assert
            ClassicAssert.AreEqual(expected, actual);
        }
    }
}