using System.Collections.Generic;
using System.Xml.Linq;

namespace ReSharperToCodeclimate
{
    public static class SeverityParser
    {
        private static readonly Dictionary<string, string> Types = new();

        public static void Parse(XElement report)
        {
            foreach (var issueType in report.Descendants("IssueType"))
            {
                var typeAttribute = issueType.Attribute("Id");
                var severityAttribute = issueType.Attribute("Severity");
                if (typeAttribute == null || severityAttribute == null)
                    continue;

                if (!Types.ContainsKey(typeAttribute.Value))
                    Types.Add(typeAttribute.Value, ConvertSeverity(severityAttribute.Value));
            }
        }

        public static string GetSeverity(string typeId)
        {
            return Types.TryGetValue(typeId, out var severity) ? severity : string.Empty;
        }

        private static string ConvertSeverity(string resharperSeverity)
        {
            return resharperSeverity switch
            {
                "ERROR" => "critical",
                "WARNING" => "major",
                "SUGGESTION" => "minor",
                "HINT" => "info",
                _ => string.Empty
            };
        }
    }
}