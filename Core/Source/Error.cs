using System;

namespace IniValidator
{
    public class Error
    {
        public Severity severity { get; private set; }
        public ErrorName name { get; private set; }
        public string message   { get; private set;}
        public int lineNumber { get; private set;}
        public Error(ErrorName name, Severity severity, string message, int lineNumber)
        {
            this.name = name;
            this.severity = severity;
            this.message = message;
            this.lineNumber = lineNumber;

            Console.WriteLine("line " + lineNumber + "(" + severity + "): " + message);
        }
    }
}