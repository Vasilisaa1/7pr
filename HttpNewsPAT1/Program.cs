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
         //   AddNews( "https://avatars.mds.yandex.net/i?id=3a1535b015537a9a1dd88e54c2a7ad8a_sr-4960885-images-thumbs&n=13","Тестовая новость", "Описание тестовой новости", cookies);
            AddNewsFromConsole(cookies); // Для синхронной версии
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

        public static void AddNews(string src, string name, string description, CookieContainer cookies)
        {
            var handler = new HttpClientHandler
            {
                CookieContainer = cookies,
                UseCookies = true,
                AllowAutoRedirect = true
            };

            using (var client = new HttpClient(handler))
            {
                string url = "http://news.permaviat.ru/ajax/add";
                var form = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("src", src),
                    new KeyValuePair<string, string>("name", name),
                    new KeyValuePair<string, string>("description", description)
                });

                Debug.WriteLine("Добавляем новую запись: " + url);

                HttpResponseMessage response =
                    client.PostAsync(url, form).Result;

                Debug.WriteLine("Статус добавления: " + response.StatusCode);

                string respText = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("Ответ сервера при добавлении:");
                Console.WriteLine(respText);
            }
        }
        public static void AddNewsFromConsole(CookieContainer cookies)
        {
            
         //   Console.WriteLine("=== ДОБАВЛЕНИЕ НОВОСТИ ===\n");

            Console.Write("Введите URL изображения: ");
            string src = Console.ReadLine();

            Console.Write("Введите заголовок новости: ");
            string name = Console.ReadLine();

            Console.Write("Введите описание новости: ");
            string description = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(src) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description))
            {
                Console.WriteLine("\nОшибка: Все поля должны быть заполнены!");
                return;
            }

         //   Console.WriteLine("\nДобавляем новость...");

            try
            {
                AddNews(src, name, description, cookies);
                Console.WriteLine("\nНовость успешно добавлена!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nОшибка: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }
    }
}
