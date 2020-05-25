using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using System.Text;

namespace tt
{
    public class Page {
        public string Url { get; set; }
        public string Content { get; set; }
    }
    class Program
    {
        static ConcurrentDictionary<string, Page> dic =  new ConcurrentDictionary<string,Page>();
        async Task<Page> Download(string url)
        {
            return new Page() {
                Url = url,
                Content = "Some Content"
            };
        }
        string [] GetUrls(Page p)
        {
            Random r = new Random();
            List<string> output = new List<string>(1000);
            for(int i = 0;i<1000;i++)
            {
                output.Add(r.Next(1,1000000).ToString());
            }
            return output.ToArray();
        }

        public async Task<Dictionary<string, Page>> crawlUrls(string [] urls)
        {
           List<Task<Task<Task<Dictionary<string,Page>>>>> pageTasks = new List<Task<Task<Task<Dictionary<string,Page>>>>>();
           foreach(var url in urls.Except(dic.Keys))
           {
               pageTasks.Add(Download(url).ContinueWith( async pt => {
                    var p = await pt;
                    System.Console.WriteLine(url);
                    dic.TryAdd(url,p);
                    var subUrls = GetUrls(p);
                    return crawlUrls(subUrls);
               }));
           }
           await Task.WhenAll(await Task.WhenAll(await Task.WhenAll(pageTasks)));
           return new Dictionary<string,Page>();
        }
        static async Task Main(string[] args)
        {
           Program p = new Program();
           string [] urls = new string []{"1","2","3"};
           var dd = await p.crawlUrls(urls);
           System.Console.WriteLine(dic.Count);
        }
    }
}