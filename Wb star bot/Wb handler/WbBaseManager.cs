using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Net;
using System.Xml;
using Wb_star_bot.Telegram_Bot;
using Wb_star_bot.Clients;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections;
using Telegram.Bot;

namespace Wb_star_bot.Wb_handler
{
    internal class WbBaseManager
    {
        private const string baseUrl = "https://suppliers-stats.wildberries.ru/api/v1/supplier/";
        private static DateTime timeNow => DateTime.UtcNow;

        private static DateTime currentDay => new DateTime(timeNow.Year, timeNow.Month, timeNow.Day);
        private static DateTime currentMounth => new DateTime(timeNow.Year, timeNow.Month, 1);
        private static DateTime lastDay => new DateTime(timeNow.Year, timeNow.Month, timeNow.Day).AddDays(-1);
        private static DateTime lastMounth => new DateTime(timeNow.Year, timeNow.Month, 1).AddMonths(-1);

        public static string IncomeToday(Bot bot, ClientData[]? client) => getData(client, "income", currentDay) ?? "Поставок за сегодня нет";
        public static string IncomeMounth(Bot bot, ClientData[]? client) => getData(client, "income", currentMounth) ?? "Поставок за текущий месяц нет";
        public static string IncomeLastToday(Bot bot, ClientData[]? client) => getData(client, "income", lastDay) ?? "Поставок за вчера нет";
        public static string IncomeLastMounth(Bot bot, ClientData[]? client) => getData(client, "income", lastMounth) ?? "Поставок за прошлый месяц нет";

        public static string OrdersToday(Bot bot, ClientData[]? client) => getData(client, "orders", currentDay) ?? "Заказов за сегодня нет";
        public static string OrdersMounth(Bot bot, ClientData[]? client) => getData(client, "orders", currentMounth) ?? "Заказов за текущий месяц нет";
        public static string OrdersLastToday(Bot bot, ClientData[]? client) => getData(client, "orders", lastDay) ?? "Заказов за вчера нет";
        public static string OrdersLastMounth(Bot bot, ClientData[]? client) => getData(client, "orders", lastMounth) ?? "Заказов за прошлый месяц нет";

        public static string SalesToday(Bot bot, ClientData[]? client) => getData(client, "sales", currentDay) ?? "Продаж за сегодня нет";
        public static string SalesMounth(Bot bot, ClientData[]? client) => getData(client, "sales", currentMounth) ?? "Продаж за текущий месяц нет";
        public static string SalesLastToday(Bot bot, ClientData[]? client) => getData(client, "sales", lastDay) ?? "Продаж за вчера нет";
        public static string SalesLastMounth(Bot bot, ClientData[]? client) => getData(client, "sales", lastMounth) ?? "Продаж за прошлый месяц нет";

        public static string GetIncomeData(Bot bot, ClientData[]? client)
        {
            if (client == null) return "";

            string content = "";

            for (int i = 0; i < client.Length; i++)
            {
                content += $"{client[i].Name}\n";

                content += $"📦 Всего поставок: {client[i].incomeData.incomes.Count()}";
            }
            return content;
        }
        public static string GetOrdersData(Bot bot, ClientData[]? client)
        {
            if (client == null) return "";

            string content = "";

            for (int i = 0; i < client.Length; i++)
            {
                content += $"{client[i].Name}\n";

                content += $"Всего заказов: {client[i].ordersData.orders.Count}";
            }
            return content;
        }

        public static string GetSalesData(Bot bot, ClientData[]? client)
        {
            if (client == null) return "";

            string content = "";

            for (int i = 0; i < client.Length; i++)
            {
                content += $"{client[i].Name}\n";

                content += $"Всего продаж: {client[i].salesData.sales.Count}";
            }
            return content;
        }
        public static string GetStocksData(Bot bot, ClientData[]? client)
        {
            if (client == null) return "";

            string content = "";

            for (int i = 0; i < client.Length; i++)
            {
                content += $"{client[i].Name}\n";

                content += client[i].stocksData.GetContent(timeNow);
            }
            return content;
        }

        public static string? getData(ClientData[]? client, string data, DateTime startTime, DateTime? endTime = null)
        {
            if (client == null)
                return null;

            string content = "";

            foreach (ClientData clientData in client)
            {
                switch (data)
                {
                    case "income":
                        content += clientData.incomeData.GetContent(startTime, endTime);
                        break;
                    case "orders":
                        content += clientData.ordersData.GetContent(startTime, endTime);
                        break;
                    case "sales":
                        content += clientData.salesData.GetContent(startTime, endTime);
                        break;
                    default:
                        return null;
                }
            }
            return content.Length > 0 ? content : null;
        }

        public static async Task update(string? apiKey, IData data, string req, string? addArg = null)
        {
            DateTime Msc = data.lastUpdate.AddHours(3);
            string date = $"{Msc.Year}-{Msc.Month}-{Msc.Day}T{Msc.Hour}:{Msc.Minute}:{Msc.Second}";
            string url = $"{baseUrl}{req}?dateFrom={date}{addArg}&key={apiKey}";
            Console.WriteLine(url);
            try
            {
                string answer = new StreamReader(WebRequest.Create(url).GetResponse().GetResponseStream()).ReadToEnd();
                data.Update(answer);
            }
            catch (WebException e)
            {
                if (e.Message == "The remote server returned an error: (400) Bad Request.")
                {
                    throw new WbException(WbException.ExceptionType.data_bad_request);
                }
                else if (e.Message == "The remote server returned an error: (429) Too Many Requests.")
                {
                    throw new WbException(WbException.ExceptionType.data_too_many_request);
                }
            }
        }

        public static string getAccountInfo(int numId)
        {
            JObject obj = JObject.Parse(new StreamReader(new HttpClient().GetAsync($"https://wbx-content-v2.wbstatic.net/sellers/{numId}.json").Result.Content.ReadAsStream()).ReadToEnd());
            return obj.GetValue("supplierName").Value<string>();
        }

        public static void getImage(int numId, string outPut)
        {
            using (WebClient client = new WebClient())
            {
                Bitmap? bmp1 = getBmp(client, 1);
                Bitmap? bmp2 = getBmp(client, 2);
                Bitmap? bmp3 = getBmp(client, 3);

                if (bmp1 != null)
                {
                    int wd = bmp1.Width;
                    int hg = bmp1.Height;

                    Bitmap bmp = new Bitmap(wd * 3, hg);

                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.DrawImage(bmp1, 0, 0, wd, hg);

                        if (bmp2 != null)
                        {
                            g.DrawImage(bmp2, wd, 0, wd, hg);

                            if (bmp3 != null)
                            {
                                g.DrawImage(bmp3, wd * 2, 0, wd, hg);
                            }
                        }
                    }

                    bmp.Save($"{outPut}{numId}.jpeg", ImageFormat.Jpeg);
                    bmp1?.Dispose();
                    bmp2?.Dispose();
                    bmp3?.Dispose();
                }
                else
                {
                    Bitmap bmp = new Bitmap(246*3, 328);
                    bmp.Save($"{outPut}{numId}.jpeg", ImageFormat.Jpeg);
                }
            }

            Bitmap? getBmp(WebClient client, int index)
            {

                int basket = 1;

                if (numId < 20000000)
                {
                    basket = 1;
                }
                else if (numId < 30000000)
                {
                    basket = 2;
                }
                else if (numId < 40000000)
                {
                    basket = 3;
                }
                else if (numId < 70000000)
                {
                    basket = 4;
                }
                else if (numId < 100000000)
                {
                    basket = 5;
                }
                else if (numId < 110000000)
                {
                    basket = 6;
                }
                else if (numId < 112000000)
                {
                    basket = 7;
                }
                else if (numId < 118000000)
                {
                    basket = 8;
                }
                else
                {
                    basket = 9;
                }
                while (true)
                {
                    try
                    {
                        string addr = $"https://basket-0{basket}.wb.ru/vol";
                        Console.WriteLine($"{addr}{numId / 100000}/part{numId / 1000}/{numId}/images/tm/{index}.jpg");
                        return new Bitmap(client.OpenRead($"{addr}{numId / 100000}/part{numId / 1000}/{numId}/images/c246x328/{index}.jpg"));
                    }
                    catch
                    {
                        if (basket >= 9)
                            return null;
                        basket++;
                    }
                }
            }
        }

        public static (string, InlineKeyboardMarkup?) GetProductPosition(Bot bot, ClientData[]? client, object? query)
        {
            bot.clientBook[(long)query].messageCallback += (a,b,c)=> Task.Run(()=> GetProductPositionCallback(a,b,c));
            return ("🔎 Введите артикул товара и поисковой запрос.\n\nПример: «_68507544 медицинский костюм_»", null);
        }

        public static async Task GetProductPositionCallback(Bot bot, ClientData[]? client, Message? message)
        {
            string[] mes = message.Text.Split(" ");
            long id = long.Parse(mes[0]);

            string category = "";
            for (int i = 1; i < mes.Length; i++)
            {
                category += mes[i] + (i == mes.Length - 1 ? "" : " ");
            }

            bot.clientBook[message.Chat.Id].messageCallback = null;
            await bot.botClient.EditMessageTextAsync(message.Chat.Id, bot.botClient.SendTextMessageAsync(message.Chat.Id, "👀 Идет поиск позиции товара...").Result.MessageId, getCategoryItems(category, id));
        }

        public static string getCategoryItems(string category, long nmId)
        {
            int position = 1;

            for (int page = 0; page < 100; page++) {
                JObject data = JObject.Parse(new StreamReader(new HttpClient().GetAsync($"https://search.wb.ru/exactmatch/ru/common/v4/search?appType=1&dest=-1029256,0,-10000000,-10000000&emp=0&lang=ru&locale=ru&page={page+1}&pricemarginCoeff=1.0&reg=0&resultset=catalog&sort=popular&suppressSpellcheck=false&query={category}").Result.Content.ReadAsStream()).ReadToEnd());
                foreach (JObject obj in data.GetValue("data").Value<JObject>().GetValue("products").Values<JObject>())
                {
                    if (nmId == obj.GetValue("id").Value<long>())
                    {
                        return $"👀 Позиция товара в поиске: {page + 1} страница, {position - page * 100} карточка.";
                    }

                    position++;
                }
            }
            return "😢 Ваш товар не ранжируется на первых 100 страницах.";
        }
        public static async void ClientDataUpdating(object arg)
        {

            Tuple<Bot, ClientData[]> args = arg as Tuple<Bot, ClientData[]>;

            while (true)
            {
                Thread.Sleep(900000);

                foreach (ClientData data in args.Item2)
                {
                    if (data.apiKey == null)
                        continue;

                    try
                    {
                        DateTime lst = data.ordersData.lastMessage;

                        await update(data.apiKey, data.incomeData, "incomes");
                        await update(data.apiKey, data.stocksData, "stocks");
                        await update(data.apiKey, data.salesData, "sales", "&flag=0");
                        await update(data.apiKey, data.ordersData, "orders", "&flag=0");

                        await data.ordersData.sendNewOrders(args.Item1, data, lst);
                    }
                    catch
                    {

                    }
                }
            }
        }
    }
}
