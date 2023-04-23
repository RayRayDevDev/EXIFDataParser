using System.Text.RegularExpressions;

namespace EXIFDataParser.Utilities
{
    internal class FileNameUtilities
    {
        public static bool IsNamingSchemeConforming(string fileName)
        {
            Regex namingSchemeRegex = new Regex(@"^\d{4}-\d{2}-\d{2} \d{2}_\d{2}_\d{2}(\s\(\d+\))?");
            return namingSchemeRegex.IsMatch(fileName);
        }

    }
}
