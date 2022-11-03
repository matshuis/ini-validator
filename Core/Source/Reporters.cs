using System;
using System.Collections.Generic;
using System.IO;

namespace IniValidator
{
    public static class HtmlReporter
    {
        public static void GenerateReport(List<IniFile> iniFiles, string rootDirectory)
        {
            Directory.CreateDirectory(rootDirectory + "\\Report");
            using (StreamWriter htmlWriter = new StreamWriter(rootDirectory + "\\Report\\index.html", false))
            {
                htmlWriter.WriteLine("<!DOCTYPE html>");
                htmlWriter.WriteLine("<html>");

                WriteBasicCss(htmlWriter);

                htmlWriter.WriteLine("<h1>Ini-validator html results</h1>");

                htmlWriter.WriteLine("<body>");

                WriteOverViewTable(htmlWriter, iniFiles);

                htmlWriter.WriteLine("<br>");

                WriteErrorTable(iniFiles, htmlWriter);

                htmlWriter.WriteLine("</body>");
                htmlWriter.WriteLine("</html>");
            }
        }

        private static void WriteErrorTable(List<IniFile> iniFiles, StreamWriter htmlWriter)
        {
            htmlWriter.WriteLine("<table id=\"ErrorTable\">");

            WriteErrorTableHeader(htmlWriter);

            foreach (var iniFile in iniFiles)
            {
                if (!iniFile.IsValidated || iniFile.Errors.Count == 0)
                {
                    continue;
                }

                var errors = iniFile.Errors;
                if (errors.Count > 0)
                {
                    foreach (var error in errors)
                    {
                        WriteErrorTableElement(htmlWriter, iniFile.FileNameWithRelativePath, error);
                    }
                }
            }

            htmlWriter.WriteLine("</table>");
        }

        private static void WriteErrorTableHeader(StreamWriter htmlWriter)
        {
            htmlWriter.WriteLine("<tr>");
            htmlWriter.WriteLine("<th>File</th>");
            htmlWriter.WriteLine("<th>Error</th>");
            htmlWriter.WriteLine("<th>Line</th>");
            htmlWriter.WriteLine("<th>Severity</th>");
            htmlWriter.WriteLine("<th>Message</th>");
            htmlWriter.WriteLine("</tr>");
        }

        private static void WriteErrorTableElement(StreamWriter htmlWriter, string fileNameWithRelativePath, Error error)
        {
            switch (error.severity)
            {
                case Severity.High:
                    htmlWriter.WriteLine("<tr style=\"background-color:#F47174\">"); // Red
                    break;
                case Severity.Medium:
                    htmlWriter.WriteLine("<tr style=\"background-color:#F4F186\">"); // yellow
                    break;
                case Severity.Low:
                    htmlWriter.WriteLine("<tr style=\"background-color:#F0F8FF\">"); // blue
                    break;
            }

            htmlWriter.WriteLine("<td>" + fileNameWithRelativePath + "/td>");
            htmlWriter.WriteLine("<td>" + error.name + "</td>");
            htmlWriter.WriteLine("<td>" + error.lineNumber + "</td>");
            htmlWriter.WriteLine("<td>" + error.severity + "</td>");
            htmlWriter.WriteLine("<td>" + error.message + "</td>");
            htmlWriter.WriteLine("</tr>");
        }

        private static void WriteOverViewTable(StreamWriter htmlWriter, List<IniFile> iniFiles)
        {
            htmlWriter.WriteLine("<table id=\"OverviewTable\">");

            WriteOverViewTableHeader(htmlWriter);
            WriteOverViewTableElement(htmlWriter, iniFiles);

            htmlWriter.WriteLine("</table>");
        }
        private static void WriteOverViewTableHeader(StreamWriter htmlWriter)
        {
            htmlWriter.WriteLine("<tr>");
            htmlWriter.WriteLine("<th>Files Scanned</th>");
            htmlWriter.WriteLine("<th>Files Valid</th>");
            htmlWriter.WriteLine("<th>Files Invalid</th>");
            htmlWriter.WriteLine("<th>Total Errors High</th>");
            htmlWriter.WriteLine("<th>Total Errors Medium</th>");
            htmlWriter.WriteLine("<th>Total Errors Low</th>");
            htmlWriter.WriteLine("</tr>");
        }

        private static void WriteOverViewTableElement(StreamWriter htmlWriter, List<IniFile> iniFiles)
        {
            htmlWriter.WriteLine("<tr>");
            htmlWriter.WriteLine("<td>" + iniFiles.Count + "</td>");
            htmlWriter.WriteLine("<td>" + GetNumberOfValidFiles(iniFiles) + "</td>");
            htmlWriter.WriteLine("<td>" + GetNumberOfInvalidFiles(iniFiles) + "</td>");
            htmlWriter.WriteLine("<td>" + GetNumberOfTotalErrors(iniFiles, Severity.High) + "</td>");
            htmlWriter.WriteLine("<td>" + GetNumberOfTotalErrors(iniFiles, Severity.Medium) + "</td>");
            htmlWriter.WriteLine("<td>" + GetNumberOfTotalErrors(iniFiles, Severity.Low) + "</td>");
            htmlWriter.WriteLine("</tr>");
        }

        private static void WriteBasicCss(StreamWriter htmlWriter)
        {
            htmlWriter.WriteLine("<head>");
            htmlWriter.WriteLine("<style>");
            htmlWriter.WriteLine("table {");
            htmlWriter.WriteLine("font-family: arial, sans-serif;");
            htmlWriter.WriteLine("border-collapse: collapse;");
            htmlWriter.WriteLine("width: 100%;");
            htmlWriter.WriteLine("}");
            htmlWriter.WriteLine("td, th {");
            htmlWriter.WriteLine("border: 1px solid #dddddd;");
            htmlWriter.WriteLine("text-align: left;");
            htmlWriter.WriteLine("padding: 8px;");
            htmlWriter.WriteLine("}");
            htmlWriter.WriteLine("tr:nth-child(even) {");
            htmlWriter.WriteLine("background-color: #dddddd;");
            htmlWriter.WriteLine("}");
            htmlWriter.WriteLine("</style>");
            htmlWriter.WriteLine("</head>");
        }

        private static string GetNumberOfValidFiles(List<IniFile> iniFiles)
        {
            int numberOfValidFiles = 0;
            foreach (var iniFile in iniFiles)
            {
                int numberOfErrorsInFile = GetTotalNumberOfErrorsInFile(iniFile);
                if (numberOfErrorsInFile == 0)
                {
                    numberOfValidFiles++;
                }
            }

            return numberOfValidFiles.ToString();
        }

        private static string GetNumberOfInvalidFiles(List<IniFile> iniFiles)
        {
            int numberOfInvalidFiles = 0;
            foreach (var iniFile in iniFiles)
            {
                int numberOfErrorsInFile = GetTotalNumberOfErrorsInFile(iniFile);
                if (numberOfErrorsInFile != 0)
                {
                    numberOfInvalidFiles++;
                }
            }

            return numberOfInvalidFiles.ToString();
        }

        private static int GetTotalNumberOfErrorsInFile(IniFile iniFile)
        {
            int numberOfErrorsInFile = 0;
            foreach (Severity severity in Enum.GetValues(typeof(Severity)))
            {
                int numberOfErrors = iniFile.GetNumberOfErrors(severity);
                numberOfErrorsInFile += numberOfErrors;
            }

            return numberOfErrorsInFile;
        }

        private static string GetNumberOfTotalErrors(List<IniFile> iniFiles, Severity severity)
        {
            int numberOfErrors = 0;
            foreach (var iniFile in iniFiles)
            {
                numberOfErrors += iniFile.GetNumberOfErrors(severity);
            }

            return numberOfErrors.ToString();
        }
    }
}