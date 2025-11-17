using System.Text.Json;

namespace D2.Model.Helper
{
    public static class PrettyPrint
    {
        public static string GetIndentedText(object input)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var serializedObject = JsonSerializer.Serialize(input, options);
            return serializedObject;
        }
    }
}