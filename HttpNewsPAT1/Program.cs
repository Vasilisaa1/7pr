using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using static System.Net.Mime.MediaTypeNames;

namespace HttpNewsPAT1
{
    public class Program
    {
        static void Main(string[] args)
        {
            WebRequest request = WebRequest.Create("http://10.111.20.114/main.php");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Console.WriteLine(response.StatusDescription);
            Stream dataStream = response.GetResponseStream();

            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            Console.WriteLine(responseFromServer);
            reader.Close();
            dataStream.Close();

            response.Close();
            Console.Read();
        }
        public static void SingIn(string Login, string Password)
        {

            string url = "http://news.permaviat.ru/ajax/login.php";
            Debug.WriteLine($"Выполняем запрос: {url}");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = new CookieContainer();

           
            string postData = $"login={Login}&password={Password}";
            byte[] Data = Encoding.ASCII.GetBytes(postData);
            request.ContentLength = Data.Length;

            
            using (var stream = request.GetRequestStream())
            {
                stream.Write(Data, 0, Data.Length);
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
          
            Debug.WriteLine($"Статус выполнения: {response.StatusCode}");

          
            string responseFromServer = new StreamReader(response.GetResponseStream()).ReadToEnd();
           
            Console.WriteLine(responseFromServer);
        }
        public static string GetContent()
        {
            string content = null;
            string url = "https://habr.com/ru/articles/";

            Debug.WriteLine($"Запрашиваем: {url}");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Debug.WriteLine($"Статус: {response.StatusCode}");

                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    content = reader.ReadToEnd();
                }
            }

            return content;
        }
        public static void ParsingHtml(string htmlCode)
        {
            var html = new HtmlDocument();
            html.LoadHtml(htmlCode);

            var document = html.DocumentNode;

           
            var articles = document.Descendants("article")
                .Where(x => x.HasClass("tm-articles-list__item"));

            foreach (var article in articles)
            {
                var titleLink = article.Descendants("a")
                    .FirstOrDefault(x => x.HasClass("tm-title__link"));
                string title = titleLink?.InnerText?.Trim() ?? "Без названия";

                string url = titleLink?.GetAttributeValue("href", "") ?? "";
                if (!string.IsNullOrEmpty(url) && !url.StartsWith("http"))
                {
                    url = "https://habr.com" + url;
                }

           
                var authorLink = article.Descendants("a")
                    .FirstOrDefault(x => x.HasClass("tm-user-info__username"));
                string author = authorLink?.InnerText?.Trim() ?? "Неизвестно";

        
                var previewDiv = article.Descendants("div")
                    .FirstOrDefault(x => x.HasClass("article-formatted-body"));
                string preview = previewDiv?.InnerText?.Trim() ?? "";
                if (preview.Length > 150) preview = preview.Substring(0, 150) + "...";

                Console.WriteLine($"{title}");
                Console.WriteLine($"Автор: {author}");
                Console.WriteLine($"{preview}");
                Console.WriteLine($"{url}");
                Console.WriteLine();
            }
        }
     
    }
}
