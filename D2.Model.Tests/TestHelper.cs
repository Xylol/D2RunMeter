using System.Reflection;

namespace D2.Model.Tests;

public static class TestHelper
{
    public static Stream ResourceStream(string resourceName)
    {
        var result = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        return result ?? throw new NullReferenceException($"Resource '{resourceName}' not found");
    }
}