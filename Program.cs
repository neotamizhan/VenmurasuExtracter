using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Threading;
using System.Xml.Schema;

namespace VenmurasuExtracter
{
    public class Entry
    {
        public int novelno { get; set; }
        public string novelname { get; set; }
        public int sectionno { get; set; }
        public string sectionname { get; set; }
        public int chapter { get; set; }
        public string published_on { get; set; }
        public string url { get; set; }
        public string jmin_url { get; set; }
        public string vmin_url { get; set; }
        public string image { get; set; }
        public string[] tags { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(
                this, 
                typeof(Entry), 
                new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            });
        }
    }

    class Program
    {
        private static Dictionary<string, int> _numbers = new Dictionary<string, int>
        {
            {"ஒன்று",1},
            {"இரண்டு",2},
            {"மூன்று",3},
            {"நான்கு",4},
            {"ஐந்து",5},
            {"ஆறு",6},
            {"ஏழு",7},
            {"எட்டு",8},
            {"ஒன்பது",9},
            {"பத்து",10},
            {"பதினொன்று",11},
            {"பன்னிரண்டு",12},
            {"பதின்மூன்று",13},
            {"பதினான்கு",14},
            {"பதினைந்து",15},
            {"பதினாறு",16},
            {"பதினேழு",17},
            {"பதினெட்டு",18},
            {"பத்தொன்பது",19},
            {"இருபது",20},
            {"இருபத்தொன்று",21},
            {"இருபத்திரண்டு",22},
            {"இருபத்திமூன்று",23},
            {"இருபத்திநான்கு",24},
            {"இருபத்திஐந்து",25}
        };

        static void Main(string[] args)
        {
           GetAllChapterLinks();
        }

        static void GetAllChapterHtml()
        {
            int linkNum = 0 ;
            int fileNum = 0;
            var Lines = new List<string>();

            var pattern = new Regex("<a href=\"(.*?)\" title=\"(.*?)\">(.*?)<\\/a>",RegexOptions.Multiline);
            var basePath = @"e:\venmurasu";
            var baseJsonPath = Path.Combine(basePath, "links.txt");

            var files = Directory.GetFiles(basePath, "*.html");

            foreach (var file in files)
            {
                fileNum++;
                linkNum = 0;

                var content = File.ReadAllText(file);
                var extract = pattern.Matches(content).ToArray();

                Lines.AddRange(from match in extract let link = match.Groups[1].Value let title = match.Groups[2].Value select $"{fileNum} ## {++linkNum} ## {link} ## {title} ##");
            }

            File.WriteAllLines(baseJsonPath, Lines, Encoding.UTF8);

        }

        static void GetAllChapterLinks()
        {
            var Entries = new List<Entry>();

            var pattern = new Regex("(.*?) ## (.*?) ## (.*?) ## (.*?) ##");
            var titlePattern = new Regex(@"நூல்\s+([\u0B80-\u0BFF ]+)\s*[-\u2013\u2014\u2015]\s*([\u0B80-\u0BFF ']+)\s*[-\u2013\u2014\u2015]\s*(\d+)");
            var basePath = @"e:\venmurasu";
            var baseJsonPath = Path.Combine(basePath, "data.json");

            var lines = File.ReadAllLines(Path.Combine(basePath, "links.txt"));

            foreach (var line in lines)
            {
                var extract = pattern.Matches(line).ToArray();

                foreach (var match in extract)
                {
                    var link = match.Groups[3].Value;
                    var title = match.Groups[4].Value;

                    var parsedTitle = titlePattern.Match(title);
                        
                    var bookNum = parsedTitle.Groups[1].Value;
                    var bookName = parsedTitle.Groups[2].Value;
                    var chapter = parsedTitle.Groups[3].Value;

                    if (chapter != string.Empty)
                    {
                        var entry = new Entry
                        {
                            novelname = bookName.Trim().Replace("\u0027",""),
                            novelno = _numbers[bookNum.Trim()],
                            chapter = int.Parse(chapter),
                            url = link
                        };

                        Entries.Add(entry);                      
                    }
                }
            }

            var orderedEntries = Entries.OrderBy(e => e.novelno).ThenBy(e => e.chapter);

            var data = JsonSerializer.Serialize(
                Entries,
                typeof(List<Entry>),
                new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                });
            File.WriteAllText(baseJsonPath, data, Encoding.UTF8);
            //File.WriteAllLines(baseJsonPath, orderedEntries.Select(e => e.ToString()), Encoding.UTF8);
        }

        static void GetAllHTML()
        {
            var basePath = @"e:\venmurasu";
            var url = "https://www.jeyamohan.in/%E0%AE%B5%E0%AF%86%E0%AE%A3%E0%AF%8D%E0%AE%AE%E0%AF%81%E0%AE%B0%E0%AE%9A%E0%AF%81/?lcp_page1=PGNUM#lcp_instance_1";
            WebClient client = new WebClient();

            for (int i = 1; i < 55; i++)
            {
                string content = client.DownloadString(url.Replace("PGNUM", i.ToString()));
                File.WriteAllText(Path.Combine(basePath, $"Chapter_{i}.html"), content, encoding: System.Text.Encoding.UTF8);
                Thread.Sleep(1000);
            }
        }
    }
}
