using System.Collections.Generic;

namespace IniValidator
{ 
    public class Section
    {
        public Section(string sectionName, int lineNumber)
        {
            SectionName = sectionName;
            LineNumber = lineNumber;
        }

        public string SectionName { get; private set; }

        public int LineNumber { get; private set; }

        public List<KeyValuePair<string, string>> keyValues = new List<KeyValuePair<string, string>>();
    }
}