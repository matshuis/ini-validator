using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IniValidator
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            CommandArgs commandArgs = new CommandArgs(args);
            bool HasDoneSomeThing = false;

            Console.WriteLine("Start ini validator");

            if (commandArgs.Help)
            {
                CommandArgs.PrintHelp();
                return;
            }

            List<IniFile> iniFiles = new List<IniFile>();
            string[] fileNames = { };

            if (commandArgs.Extensions.Length > 0)
            {
                Console.WriteLine("GetFilesByExtensions " + commandArgs.Directory);
                fileNames = GetFilesByExtensions(commandArgs.Directory, commandArgs.Extensions);
            }
            else if (!string.IsNullOrEmpty(commandArgs.FileList))
            {
                Console.WriteLine("filelist " + commandArgs.FileList);
                fileNames = GetFileNames(commandArgs.FileList);
            }

            if (fileNames.Length > 0)
            {
                HasDoneSomeThing = true;
                foreach (var file in fileNames)
                {
                    if (string.IsNullOrEmpty(file))
                    {
                        continue;
                    }

                    var iniFile = new IniFile(file, commandArgs.Directory);
                    iniFile.Validate();
                    iniFiles.Add(iniFile);
                }
            }

            if (HasDoneSomeThing)
            {
                Directory.CreateDirectory(commandArgs.Directory + "\\report");
                HtmlReporter.GenerateReport(iniFiles, commandArgs.Directory);
                Console.WriteLine("validated " + iniFiles.Count + " files");
            }
            else
            {
                CommandArgs.PrintHelp();
            }
        }

        private static string[] GetFilesByExtensions(string dir, params string[] extensions)
        {
            string[] files = { };
            foreach (var ex in extensions)
            {
                var extensionFiles = Directory.GetFiles(dir, ex, SearchOption.AllDirectories);
                files = files.Concat(extensionFiles).ToArray();
            }

            return files;
        }

        private static string[] GetFileNames(string fileListFile)
        {
            string file = File.ReadAllText(fileListFile);
            string[] lines = file.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            return lines;
        }
    }
}
