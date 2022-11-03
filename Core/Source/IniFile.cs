using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace IniValidator
{
    public class IniFile
    {
        private readonly List<Section> sections = new List<Section>();
        private readonly string fileNameWithPath;
        private string fileNameWithRelativePath;
        private List<Error> errors = new List<Error>();

        public bool IsValidated { get; private set; } = false;
        public List<Error> Errors { get => errors; private set => errors = value; }
        public string FileNameWithRelativePath { get => fileNameWithRelativePath; private set => fileNameWithRelativePath = value; }

        public IniFile(string file, string rootDirectoy)
        {
            this.fileNameWithRelativePath = file.Replace(rootDirectoy + "\\", "");
            this.fileNameWithPath = file;
        }
        
        public void Validate()
        {
            if (!string.IsNullOrEmpty(fileNameWithPath))
            {
                LineContext lineContext = new LineContext();
                Console.WriteLine("Parsing ini file " + fileNameWithPath);
                string iniFile;
                try
                {
                    iniFile = File.ReadAllText(fileNameWithPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Can not parse ini file " + fileNameWithPath + " does it exists? " + ex.Message);
                    return;
                }
                
                string[] lines = iniFile.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                string currentSectionName = "";
                string currentKeyName = "";
                foreach (var line in lines)
                {
                    lineContext.lineNumber++;
                    lineContext.line = line;

                    if (lineContext.nextLineIsSameItem)
                    {
                        HandleNextLineIsSameItem(ref lineContext, currentSectionName, currentKeyName);
                    }
                    else if (IsLineSection(lineContext.line))
                    {
                        currentSectionName = HandleLineSection(ref lineContext, ref currentKeyName);
                    }
                    else if (IsLineComment(line))
                    {
                        HandleLineComment(ref lineContext);
                    }
                    else if (IsLineKeyValue(lineContext.line))
                    {
                        HandleLineKeyValue(ref lineContext, currentSectionName, ref currentKeyName);
                    }
                    else if (IsLineEncoded(lineContext.line))
                    {
                        string errorLine = "Can not parse line. It might be encoded.";
                        errors.Add(new Error(ErrorName.ParseLine, Severity.Medium, errorLine, lineContext.lineNumber));
                    }
                    else if (IsLineSomeThingElse(lineContext.line))
                    {
                        string errorLine = "Can not parse line: " + lineContext.line;
                        errors.Add(new Error(ErrorName.ParseLine, Severity.High, errorLine, lineContext.lineNumber));
                    }

                }
            }
            else
            {
                Console.WriteLine("Can not find " + fileNameWithPath);
            }

            IsValidated = true;
        }

        private void ConcatKeyValue(string currentSectionName, string currentKeyName, LineContext lineContext, string concatValue)
        {
            var section = sections.Find(s => (s.SectionName == currentSectionName));
            if (section == null)
            {
                string errorLine = " has no section " + lineContext.line;
                errors.Add(new Error(ErrorName.NoSection, Severity.High, errorLine, lineContext.lineNumber));
                return;
            }

            // add to latest key/value string
            var element = section.keyValues.FirstOrDefault(kv => kv.Key == currentKeyName);
            if (element.Equals(default(KeyValuePair<string, string>)))
            {
                string errorLine = " has no key " + lineContext.line;
                errors.Add(new Error(ErrorName.NoKey, Severity.High, errorLine, lineContext.lineNumber));
                return;
            }
            var attrIndex = section.keyValues.FindIndex(kv => kv.Key == currentKeyName);
            if (attrIndex == -1)
            {
                string errorLine = " Can not find key " + currentKeyName;
                errors.Add(new Error(ErrorName.CanNotFindKey, Severity.High, errorLine, lineContext.lineNumber));
                return;
            }

            string newAttrbValue = element.Value + "\r\n" + concatValue;
            section.keyValues[attrIndex] = new KeyValuePair<string, string>(element.Key, newAttrbValue);
        }

        private void HandleNextLineIsSameItem(ref LineContext lineContext, string currentSectionName, string currentKeyName)
        {
            lineContext.nextLineIsSameItem = false;
            string value = lineContext.line;
            if (IsNextLineSameItem(lineContext.line))
            {
                lineContext.nextLineIsSameItem = true;
                value = value.Remove(value.IndexOf('\\')); // remove trailing \ and comment
            }

            ConcatKeyValue(currentSectionName, currentKeyName, lineContext, value);
        }

        private void HandleLineKeyValue(ref LineContext lineContext, string currentSectionName, ref string currentKeyName)
        {
            if (IsNextLineSameItem(lineContext.line))
            {
                lineContext.nextLineIsSameItem = true;
            }

            string[] keyValue = lineContext.line.Split('=');
            string replacement = Regex.Replace(keyValue[0], @"\t| ", ""); // Trim key
            string key = replacement;
            string value = keyValue[1];

            var section = sections.Find(s => (s.SectionName == currentSectionName));
            if (section == null)
            {
                errors.Add(new Error(ErrorName.ItemWithoutSection, Severity.Low, "Item doesn't have section yet", lineContext.lineNumber));
                return;
            }
            if (section.keyValues.Any(kv => kv.Key == key))
            {
                string errorLine = "Already has ini key " + key + " in section [" + currentSectionName + "]";
                errors.Add(new Error(ErrorName.DuplicateKey, Severity.High, errorLine, lineContext.lineNumber));
                return;
            }
            currentKeyName = key;
            section.keyValues.Add(new KeyValuePair<string, string>(key, value));
        }
       
        private string HandleLineSection(ref LineContext lineContext, ref string currentKeyName)
        {
            string currentSectionName = "";
            lineContext.nextLineIsSameItem = false;
            int startIndex = lineContext.line.IndexOf('[') + 1;
            int length = lineContext.line.IndexOf(']') - startIndex;
            if (length <= 0)
            {
                string errorLine = "wrong sectionName " + lineContext.line;
                errors.Add(new Error(ErrorName.WrongSection, Severity.High, errorLine, lineContext.lineNumber));
            }
            else
            {
                currentSectionName = lineContext.line.Substring(startIndex, length);
                if (sections.Any(s => currentSectionName == s.SectionName))
                {
                    var duplicate = sections.Find(s => currentSectionName == s.SectionName);
                    string errorMessage = "";
                    if (duplicate != null)
                    {
                        errorMessage = "Already has sectionName " + currentSectionName + " on line " + duplicate.LineNumber;
                    }
                    else
                    {
                        errorMessage = "Already has sectionName " + currentSectionName;
                    }
                    errors.Add(new Error(ErrorName.DuplicateSection, Severity.High, errorMessage, lineContext.lineNumber));
                }
                else
                {
                    sections.Add(new Section(currentSectionName, lineContext.lineNumber));
                    currentKeyName = "";
                }
            }

            return currentSectionName;
        }

        private static void HandleLineComment(ref LineContext lineContext)
        {
            lineContext.nextLineIsSameItem = false;
        }

        private static bool IsNextLineSameItem(string line)
        {
            string trimmedValue = Regex.Replace(line, @"\t|\n|\r| ", "");
            if (trimmedValue.LastIndexOf("//") >= 0)
            {
                trimmedValue = trimmedValue.Remove(trimmedValue.LastIndexOf("//"));
            }
            return trimmedValue.EndsWith("\\");
        }

        private static bool IsLineKeyValue(string line)
        {
            return line.Contains('=');
        }

        private static bool IsLineSection(string line)
        {
            int startIndex = line.IndexOf('[');
            int endIndexIndex = line.IndexOf(']');
            if (startIndex == 0 && endIndexIndex >= 0 && endIndexIndex > startIndex)
            {
                return true;
            }
            
            return false;
        }

        private bool IsLineEncoded(string line)
        {
            return line.Contains('�');
        }

        private bool IsLineSomeThingElse(string line)
        {
            string trimmedValue = Regex.Replace(line, @"\t|\n|\r| ", "");
            if (trimmedValue.LastIndexOf("//") >= 0)
            {
                trimmedValue = trimmedValue.Remove(trimmedValue.LastIndexOf("//"));
            }

            return trimmedValue.Length > 0;
        }

        private static bool IsLineComment(string line)
        {
            string replacement = Regex.Replace(line, @"\t|\n|\r| ", "");
            return replacement.StartsWith("//") || replacement.StartsWith(";");
        }

        public int GetNumberOfErrors(Severity severity)
        {
            var numberOfErrors = (from error in errors where (error.severity == severity) select error).Count();
            return numberOfErrors;
        }

        public string GetNumberOfFailures()
        {
            var numberOfFailures = (from error in errors where (error.severity == Severity.Low || error.severity == Severity.Medium) select error).Count();
            return numberOfFailures.ToString();
        }
    }
}
