using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace ReSharperToCodeclimate
{
    class Program
    {
        static void Main(string[] args)
        {
            JArray codeClimateReport = new JArray();

            XElement resharperReport = XElement.Load(args[0]);
            SeverityParser.Parse(resharperReport);

            foreach (XElement issue in resharperReport.Descendants("Issue"))
            {
                codeClimateReport.Add(
                    new JObject(
                        new JProperty("description", issue.Attribute("Message").Value),
                        new JProperty("severity", SeverityParser.GetSeverity(issue.Attribute("TypeId").Value)),
                        new JProperty("fingerprint", CalculateFingerprint(issue)),
                        new JProperty("location", new JObject(
                            new JProperty("path", issue.Attribute("File").Value.Replace("\\", "/")),
                            new JProperty("lines", new JObject(
                                new JProperty("begin", issue.Attribute("Line")?.Value ?? "0")))
                        ))
                    )
                );
            }

            File.WriteAllText(args[1], codeClimateReport.ToString());
        }

        private static string CalculateFingerprint(XElement issue)
        {
            string input = issue.Attribute("File").Value + "-" + issue.Attribute("Offset").Value + '-' + issue.Attribute("TypeId").Value;

            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(data).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}