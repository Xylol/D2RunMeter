using System.IO;
using System.Reflection;

namespace D2.Model.Tests
{
    public static class TestHelper
    {
        public static Stream ResourceStream(string resourceName)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        }

        public static string[] ResourceNames()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames();
        }
    }
}