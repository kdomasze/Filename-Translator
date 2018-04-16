using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace FileNameTranslator
{
    public class Program
    {
        private static string _sourceLang = "auto";
        private static string _targetLang = "en";
        private static string _path = Directory.GetCurrentDirectory();
        private static bool _straightRename = false;
        private static bool _verbose = false;

        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            ParseArgs(args);

            var sourceText = "";

            Log("Opening path: " + _path);
            if (!Directory.Exists(_path))
            {
                Log("Path does not exist. Exiting.");
                return;
            }

            var fileDetailsList = new List<FileDetails>();

            var files = Directory.GetFiles(_path).ToList();

            foreach (var file in files)
            {
                Log("Parsing file: " + file);
                var fileDetails = new FileDetails()
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    Extension = Path.GetExtension(file)
                };

                fileDetailsList.Add(fileDetails);

                sourceText += fileDetails.Name;
                if (files.Last() != file)
                {
                    sourceText += "\n";
                }
            }

            // translate url
            var url = "https://translate.googleapis.com/translate_a/single?client=gtx&sl="
            + _sourceLang + "&tl=" + _targetLang + "&dt=t&q=" + HttpUtility.UrlEncode(sourceText);

            var uri = new Uri(url);

            // download translation to tmp file
            var tmpFile = Path.GetTempFileName();
            Log("Getting translation for file names.");
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 " +
                                              "(KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
                client.DownloadFile(uri, tmpFile);
            }

            // parse translation
            if (File.Exists(tmpFile))
            {
                Log("Parsing translation.");
                var result = File.ReadAllText(tmpFile).Split('"');

                var fileDetailsIndex = 0;
                var index = 0;
                var nextIndex = 1;

                foreach (var item in result)
                {
                    if (index == nextIndex)
                    {
                        var translation = item;
                        if (item.Contains("\\n"))
                            translation = item.Substring(0, item.Length - 2);

                        fileDetailsList[fileDetailsIndex].Translation = translation;

                        if (fileDetailsIndex == fileDetailsList.Count - 1) break;
                        nextIndex += 4;
                        fileDetailsIndex++;
                    }

                    index++;
                }
            }
            else
            {
                Console.WriteLine("Translation failed. Exiting.");
                return;
            }

            // rename
            foreach (var fileDetail in fileDetailsList)
            {
                var oldFileName = Path.Combine(_path, fileDetail.Name + fileDetail.Extension);
                var newFileName = "";

                if (_straightRename)
                {
                    newFileName = Path.Combine(_path, fileDetail.Translation + fileDetail.Extension);
                }
                else
                {
                    newFileName = Path.Combine(_path, fileDetail.Translation + " (" + fileDetail.Name + ")" + fileDetail.Extension);
                }
                Log("Renaming file \"" + oldFileName + "\" to \"" + newFileName + "\"");
                File.Move(oldFileName, newFileName);
            }

            Console.WriteLine("Translation complete.");
        }

        private static void Log(string message)
        {
            if (_verbose) Console.WriteLine("Opening path: " + message);
        }


        /// <summary>
        /// Parses the commandline arguments.
        /// </summary>
        /// <param name="args">The list of commandline arguments</param>
        private static void ParseArgs(string[] args)
        {
            if (args.Length != 0 && !args[0].Contains('-')) _path = args[0];

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--sourcelang":
                    case "-sl":
                        _sourceLang = args[i + 1];
                        break;
                    case "--targetlang":
                    case "-tl":
                        _targetLang = args[i + 1];
                        break;
                    case "--straightRename":
                    case "-sr":
                        _straightRename = true;
                        break;
                    case "--verbose":
                    case "-v":
                        _verbose = true;
                        break;
                    case "--help":
                    case "-h":
                        PrintHelp();
                        return;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Prints the help text to console.
        /// </summary>
        private static void PrintHelp()
        {
            Console.WriteLine("File Name Translator 1.0, a simple file name translator and renamer.");
            Console.WriteLine("Usage: fnt [OPTION]...");
            Console.WriteLine("       fnt [PATH] [OPTION]...");
            Console.WriteLine("If no path is specified, the current working directory will be the root of renaming.");
            Console.WriteLine("Options:");
            Console.WriteLine("\t-sl, --sourcelang [lang]\t\tspecifies the source language of the file names (default: auto).");
            Console.WriteLine("\t-tl, --targetlang [lang]\t\tspecifies the output language of the file names (default: en).");
            Console.WriteLine("\t-sr, --straightRename\t\t specifies whether to rename the files directly or keep the original name in the new name (default: [translation] ([original name]).[ext]).");
            Console.WriteLine("\t-v, --verbose\t\tprints extra details about the process during the scripts execution.");
            Console.WriteLine("\t-h, --help\t\tdisplays this help page.");
        }
    }

    public class FileDetails
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Translation { get; set; }
    }
}
