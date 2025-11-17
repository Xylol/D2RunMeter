using D2.Model.Helper;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace D2.Model.Tests
{
    [TestFixture]
    public class PrettyPrintTests
    {
        public class Person
        {
            public Person(string name, int age)
            {
                this.Name = name;
                this.Age = age;
            }

            public string Name { get; set; }
            public int Age { get; set; }
        }

        [Test]
        public void PrettyPrint_WhenJohnDoeObject_ExpectPrettyJsonPrint()
        {
            // Arrange
            var testPerson = new Person("John Doe", 33);
            var expected = $"{{{System.Environment.NewLine}  \"Name\": \"John Doe\",{System.Environment.NewLine}  \"Age\": 33{System.Environment.NewLine}}}";
            // Act
            var actual = PrettyPrint.GetIndentedText(testPerson);

            // Assert
            ClassicAssert.AreEqual(expected, actual);
        }
    }
}