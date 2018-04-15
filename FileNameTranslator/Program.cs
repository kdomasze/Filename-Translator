using System;
using System.IO;
using System.Net;
using System.Web;

namespace FileNameTranslator
{
    public class Program
    {
        //private static HttpClient Client = new HttpClient();

        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            // url vars
            var sourceText = "こんにちは世界";
            var sourceLang = "auto";
            var targetLang = "en";
            
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

            // output to console
            if(File.Exists(tmpFile))
            {
                string result = File.ReadAllText(tmpFile);
                Console.WriteLine(result);
            }
            else
            {
                Console.WriteLine("failed");
            }
            Console.ReadKey();
        }
    }
}
