﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace ReSharperToCodeClimate
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 2)
            {
                Console.WriteLine("Usage: dotnet resharper-to-codeclimate imput.xml output.json");
                Environment.Exit(1);
            }

            List<CodeClimateIssue> codeClimateReport = new List<CodeClimateIssue>();

            XElement reSharperReport = XElement.Load(args[0]);
            Dictionary<string, string> severityByIssueType = CreateSeverityByIssueTypeDictionary(reSharperReport.Descendants("IssueType"));

            foreach (XElement issue in reSharperReport.Descendants("Issue"))
            {
                codeClimateReport.Add(
                    new CodeClimateIssue()
                    {
                        Description = issue.Attribute("Message").Value,
                        Severity = severityByIssueType[issue.Attribute("TypeId").Value],
                        Fingerprint = CalculateFingerprint(issue),
                        Location = new IssueLocation()
                        {
                            Path = issue.Attribute("File").Value.Replace("\\", "/"),
                            Lines = new LineRange()
                            {
                                Begin = int.Parse(issue.Attribute("Line")?.Value ?? "0")
                            }
                        }
                    }
                );
            }

            JsonSerializerOptions options = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            File.WriteAllText(args[1], JsonSerializer.Serialize(codeClimateReport, options));
        }

        private static Dictionary<string, string> CreateSeverityByIssueTypeDictionary(IEnumerable<XElement> issueTypes)
        {
            Dictionary<string, string> codeClimateSeverityByReSharperSeverity = new Dictionary<string, string>()
            {
                {"ERROR", "critical"},
                {"WARNING", "major"},
                {"SUGGESTION", "minor"},
                {"HINT", "info"}
            };

            Dictionary<string, string> serverityByIssueType = new Dictionary<string, string>();

            foreach (var issueType in issueTypes)
            {
                string severity = codeClimateSeverityByReSharperSeverity[issueType.Attribute("Severity").Value];
                serverityByIssueType.Add(issueType.Attribute("Id").Value, severity);
            }

            return serverityByIssueType;
        }

        private static ConcurrentDictionary<string, int> FingerprintCounters = new();

        private static string CalculateFingerprint(XElement issue)
        {
            var file = issue.Attribute("File").Value;
            var type = issue.Attribute("TypeId").Value;

            var counter = FingerprintCounters.AddOrUpdate(file + '-' + type, 1, (_, count) => count + 1);

            string input = file + "-" + counter + '-' + type;

            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(data).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    class LineRange
    {
        public int Begin { get; set; }
    }

    class IssueLocation
    {
        public string Path { get; set; }

        public LineRange Lines { get; set; }
    }

    class CodeClimateIssue
    {
        public string Description { get; set; }

        public string Fingerprint { get; set; }

        public string Severity { get; set; }

        public IssueLocation Location { get; set; }
    }
}
