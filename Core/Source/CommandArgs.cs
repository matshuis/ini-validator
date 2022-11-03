using System;

namespace IniValidator
{
    public class CommandArgs
    {
        public CommandArgs(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.Trim().StartsWith("-filelist="))
                {
                    string[] strings = arg.Split('=');
                    FileList = strings[1];
                }
                if (arg.Trim().StartsWith("-directory="))
                {
                    string[] strings = arg.Split('=');
                    Directory = strings[1];
                }
                if (arg.Trim().StartsWith("-extensions="))
                {
                    string[] strings = arg.Split('=');
                    Extensions = strings[1].Split(',');
                }
                else if (arg.Trim().ToLower() == "-help")
                {
                    Help = true;
                }
            }
        }

        public static void PrintHelp()
        {
            Console.WriteLine("command-line app to validate ini file syntax.");
            Console.WriteLine("-help for help");
            Console.WriteLine("-directory=<\"C:\\temp\"> Base directory of the repository");
            Console.WriteLine("-filelist=<C:\\temp\\filelist.txt>. Can no be used in combination with -extensions");
            Console.WriteLine("-extensions=<\"*.abc,*.ini\"> used in combination with -directory. It scans this directory with sub directories on these extensions. Can not be used in combination with -filelist");
        }

        public string FileList { get; private set; } = "";

        public string Directory { get; private set; } = "";

        public string[] Extensions { get; private set; } = { };

        public bool Help { get; private set; } = false;
    }
}