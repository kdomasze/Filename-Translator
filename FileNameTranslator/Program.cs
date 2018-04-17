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
        #region Flags
        private static string _sourceLang = "auto";
        private static string _targetLang = "en";
        private static string _path = Directory.GetCurrentDirectory();
        private static bool _straightRename = false;
        private static bool _verbose = false;
        #endregion

        public static void Main(string[] args)
        {
            #region Initialize
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            ParseArgs(args);

            var sourceText = "";

            Log("Opening path: " + _path);
            if (!Directory.Exists(_path))
            {
                Log("Path does not exist. Exiting.");
                return;
            }

            var tmpFile = Path.GetTempFileName();
            var fileDetailsList = new List<FileDetails>();

            var files = Directory.GetFiles(_path).ToList();

            if(files.Count == 0)
            {
                Log("No files exist. Exiting.");
                return;
            }

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
            #endregion

            #region Download translation
            Log("Getting translation for file names.");
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 " +
                                              "(KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
                client.DownloadFile(uri, tmpFile);
            }
            #endregion

            #region  Parse translation
            if (File.Exists(tmpFile))
            {
                Log("Parsing translation.");
                var result = File.ReadAllText(tmpFile).Split('"');

                var fileDetailsIndex = 0;
                var index = 0;
                var nextIndex = 1;

                // The sections of interest will be on every 4th line after starting from line 2.
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
            #endregion

            #region Rename
            foreach (var fileDetail in fileDetailsList)
            {
                var oldFileName = Path.Combine(_path, fileDetail.Name + fileDetail.Extension);
                var newFileName = "";
                
                // replaces illegal filename characters with alternatives
                fileDetail.Translation = fileDetail.Translation.Replace('/', '-');
                fileDetail.Translation = fileDetail.Translation.Replace('\\', '-');
                fileDetail.Translation = fileDetail.Translation.Replace('|', '-');
                fileDetail.Translation = fileDetail.Translation.Replace('<', '(');
                fileDetail.Translation = fileDetail.Translation.Replace('>', ')');
                fileDetail.Translation = fileDetail.Translation.Replace(':', '-');
                fileDetail.Translation = fileDetail.Translation.Replace('\"', '\'');
                fileDetail.Translation = fileDetail.Translation.Replace("?", "");
                fileDetail.Translation = fileDetail.Translation.Replace("*", "");

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
            #endregion

            Console.WriteLine("Translation complete.");
        }

        /// <summary>
        /// Prints message to the console if user specifies the verbose flag.
        /// </summary>
        /// <param name="message">String to print</param>
        private static void Log(string message)
        {
            if (_verbose) Console.WriteLine(message);
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
                        Environment.Exit(0);
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
            Console.WriteLine("\t-sl, --sourcelang [lang]\tspecifies the source language of the file names (default: auto).");
            Console.WriteLine("\t-tl, --targetlang [lang]\tspecifies the output language of the file names (default: en).");
            Console.WriteLine("\t-sr, --straightRename\t\tspecifies whether to rename the files directly or keep the original\n\t\t\t\t\t\tname in the new name (default: [translation] ([original name]).[ext]).");
            Console.WriteLine("\t-v, --verbose\t\t\tprints extra details about the process during the scripts execution.");
            Console.WriteLine("\t-h, --help\t\t\tdisplays this help page.");
        }
    }

    public class FileDetails
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Translation { get; set; }
    }
}
