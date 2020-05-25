using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using System.Text;
using System.Diagnostics;

namespace tt
{
    public class Page {
        public string Url { get; set; }
        public string Content { get; set; }
        public List<string> Links {get;set;} 
    }
    class Program
    {
        static ConcurrentDictionary<string, Page> dic =  new ConcurrentDictionary<string,Page>();
        async Task<Page> Download(string url)
        {
            Random r = new Random();
            return new Page() {
                Url = url,
                Content = "Some Content",
                Links = Enumerable.Range(1,10).Select(x => r.Next(1,10000).ToString()).ToList()
            };
        }
        IEnumerable<string> GetUrls(Page p)
        {
           return p.Links;
        }
        
        public async Task<Dictionary<string, Page>> crawlUrlsVersionOne(IEnumerable<string> urls)
        {
           List<Task<Task>> pageTasks = new List<Task<Task>>();
           foreach(var url in urls.Except(dic.Keys))
           {
               pageTasks.Add(Download(url).ContinueWith( async pt => {
                    var p = await pt;
                    dic.TryAdd(url,p);
                    var subUrls = GetUrls(p);
                    await crawlUrlsVersionOne(subUrls);
               }));
           }
           await Task.WhenAll(await Task.WhenAll(pageTasks));
           return new Dictionary<string,Page>();
        }

        public async Task<Dictionary<string, Page>> crawlUrlsVersionTwo(IEnumerable<string> urls)
        {
           List<Task<Task<Task<Dictionary<string,Page>>>>> pageTasks = new List<Task<Task<Task<Dictionary<string,Page>>>>>();
           foreach(var url in urls.Except(dic.Keys))
           {
               pageTasks.Add(Download(url).ContinueWith( async pt => {
                    var p = await pt;
                    dic.TryAdd(url,p);
                    var subUrls = GetUrls(p);
                    return crawlUrlsVersionTwo(subUrls);
               }));
           }
           await Task.WhenAll(await Task.WhenAll(await Task.WhenAll(pageTasks)));
           return new Dictionary<string,Page>();
        }
        static async Task Main(string[] args)
        {
           Program p = new Program();
           string [] urls = new string []{"1","2","3"};
           Stopwatch sw = new Stopwatch();
           sw.Start();
           await p.crawlUrlsVersionOne(urls);
           sw.Stop();
           System.Console.WriteLine(sw.Elapsed.TotalSeconds);
           dic.Clear();
           sw.Restart();
           await p.crawlUrlsVersionTwo(urls);
           sw.Stop();
           System.Console.WriteLine(sw.Elapsed.TotalSeconds);
           System.Console.WriteLine(dic.Count);
        }
    }
}