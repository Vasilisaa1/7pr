using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack;

namespace HttpNewsPAT1
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Авторизация, получаем контейнер с куками
            CookieContainer cookies = SingIn("user", "user");

            // Получаем страницу с новостями
            string content = GetContent(cookies);

            // Парсим HTML
            ParsingHtml(content);

            // Добавляем новость (для оценки «Хорошо»)
            AddNews("Тестовая новость", "Описание тестовой новости", cookies);

            Console.WriteLine("Готово. Нажмите любую клавишу...");
            Console.ReadKey();
        }

        public static void ParsingHtml(string htmlCode)
        {
            var html = new HtmlDocument();
            html.LoadHtml(htmlCode);

            var document = html.DocumentNode;
            var divNews = document.Descendants()
                                  .Where(x => x.HasClass("news"));

            bool firstNews = true;

            foreach (var divNew in divNews)
            {
                if (!firstNews)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                }
                firstNews = false;

                var src = divNew.ChildNodes[1].GetAttributeValue("src", "none");
                var name = divNew.ChildNodes[3].InnerHtml;
                var description = divNew.ChildNodes[5].InnerHtml;

                Console.WriteLine("{0}\nИзображение: {1}\nОписание: {2}",
                    name, src, description);
            }
        }

        // -------- HttpClient + CookieContainer --------

        public static CookieContainer SingIn(string login, string password)
        {
            var cookies = new CookieContainer();

            var handler = new HttpClientHandler
            {
                CookieContainer = cookies,
                UseCookies = true,
                AllowAutoRedirect = true
            };

            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

                var form = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("login", login),
                    new KeyValuePair<string, string>("password", password)
                });

                Debug.WriteLine("Выполняем запрос авторизации: http://news.permaviat.ru/ajax/login.php");

                HttpResponseMessage response =
                    client.PostAsync("http://news.permaviat.ru/ajax/login.php", form).Result;

                Debug.WriteLine("Статус выполнения: " + response.StatusCode);

                string respText = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("Ответ сервера при авторизации:");
                Console.WriteLine(respText);
            }

            // cookies теперь содержит куки с токеном
            return cookies;
        }

        public static string GetContent(CookieContainer cookies)
        {
            var handler = new HttpClientHandler
            {
                CookieContainer = cookies,
                UseCookies = true,
                AllowAutoRedirect = true
            };

            using (var client = new HttpClient(handler))
            {
                Debug.WriteLine("Выполняем запрос: http://news.permaviat.ru/main");

                HttpResponseMessage response =
                    client.GetAsync("http://news.permaviat.ru/main").Result;

                Debug.WriteLine("Статус выполнения: " + response.StatusCode);

                string content = response.Content.ReadAsStringAsync().Result;

                // При желании можно закомментировать, чтобы не засорять консоль
                // Console.WriteLine(content);

                return content;
            }
        }

        public static void AddNews(string title, string description, CookieContainer cookies)
        {
            var handler = new HttpClientHandler
            {
                CookieContainer = cookies,
                UseCookies = true,
                AllowAutoRedirect = true
            };

            using (var client = new HttpClient(handler))
            {
                var form = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("title", title),
                    new KeyValuePair<string, string>("description", description)
                });

                Debug.WriteLine("Добавляем новую запись: http://news.permaviat.ru/add");

                HttpResponseMessage response =
                    client.PostAsync("http://news.permaviat.ru/add", form).Result;

                Debug.WriteLine("Статус добавления: " + response.StatusCode);

                string respText = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("Ответ сервера при добавлении:");
                Console.WriteLine(respText);
            }
        }
    }
}
