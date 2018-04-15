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
        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            // url vars
            var sourceText = "";
            var sourceLang = "auto";
            var targetLang = "en";

            var path = Path.Combine(Directory.GetCurrentDirectory(), "test");

            if (!Directory.Exists(path)) return;

            var fileDetailsList = new List<FileDetails>();

            var files = Directory.GetFiles(path).ToList();

            foreach(var file in files)
            {
                var fileDetails = new FileDetails()
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    Extension = Path.GetExtension(file)
                };

                fileDetailsList.Add(fileDetails);

                sourceText += fileDetails.Name;
                if(files.Last() != file)
                {
                    sourceText += "\n";
                }
            }

            // translate url
            var url = "https://translate.googleapis.com/translate_a/single?client=gtx&sl="
            + sourceLang + "&tl=" + targetLang + "&dt=t&q=" +  HttpUtility.UrlEncode(sourceText);

            var uri = new Uri(url);

            // download translation to tmp file
            var tmpFile = Path.GetTempFileName();
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 " +
                                              "(KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
                client.DownloadFile(uri, tmpFile);
            }

            // parse translation
            if(File.Exists(tmpFile))
            {
                var result = File.ReadAllText(tmpFile).Split('"');

                var fileDetailsIndex = 0;
                var index = 0;
                var nextIndex = 1;

                foreach(var item in result)
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
                Console.WriteLine("failed");
            }

            // rename
            foreach(var fileDetail in fileDetailsList)
            {
                var oldFileName = Path.Combine(path, fileDetail.Name + fileDetail.Extension);
                var newFileName = Path.Combine(path, fileDetail.Translation + fileDetail.Extension);
                File.Move(oldFileName, newFileName);
            }

            Console.ReadKey();
        }
    }

    public class FileDetails
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Translation { get; set; }
    }
}
