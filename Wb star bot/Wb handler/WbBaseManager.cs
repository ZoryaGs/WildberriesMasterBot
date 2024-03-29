﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Xml;
using Wb_star_bot.Telegram_Bot;
using Wb_star_bot.Clients;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.InputFiles;
using System.Collections;
using Telegram.Bot;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

using File = System.IO.File;

#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8603
#pragma warning disable CS8604
#pragma warning disable CS8605

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

        public delegate void onFinished();

        public static string output => $"{dataDirectory}/";
        public static string dataDirectory => $"{Directory.GetCurrentDirectory()}/data";


        public static long[] baskets = new long[]
        {
            20000000,
            30000000,
            40000000,
            75000000,
            100000000,
            110000000,
            115000000,
            120000000,
        };

        #region OldVer 
        /*
public static string OrdersToday(Bot bot, ClientData[]? client) => getData(client, "orders", currentDay) ?? "Заказов за сегодня нет";
public static string OrdersMounth(Bot bot, ClientData[]? client) => getData(client, "orders", currentMounth) ?? "Заказов за текущий месяц нет";
public static string OrdersLastToday(Bot bot, ClientData[]? client) => getData(client, "orders", lastDay) ?? "Заказов за вчера нет";
public static string OrdersLastMounth(Bot bot, ClientData[]? client) => getData(client, "orders", lastMounth) ?? "Заказов за прошлый месяц нет";

public static string IncomeToday(Bot bot, ClientData[]? client) => getData(client, "income", currentDay) ?? "Поставок за сегодня нет";
public static string IncomeMounth(Bot bot, ClientData[]? client) => getData(client, "income", currentMounth) ?? "Поставок за текущий месяц нет";
public static string IncomeLastToday(Bot bot, ClientData[]? client) => getData(client, "income", lastDay) ?? "Поставок за вчера нет";
public static string IncomeLastMounth(Bot bot, ClientData[]? client) => getData(client, "income", lastMounth) ?? "Поставок за прошлый месяц нет";

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
        content += $"{client[i].Smile} {client[i].Name}\n";

        content += $"📦 Всего поставок: {client[i].incomeData.incomes.Count()}";
    }
    return content;
}


public static string GetSalesData(Bot bot, ClientData[]? client)
{
    if (client == null) return "";

    string content = "";

    for (int i = 0; i < client.Length; i++)
    {
        content += $"{client[i].Smile} {client[i].Name}\n";

        content += $"Всего продаж: {client[i].salesData.sales.Count}";
    }
    return content;
}
*/
        #endregion

        public static string GetOrdersData(Bot bot, ClientData[]? client)
        {
            if (client == null) return "";

            string content = "";

            for (int i = 0; i < client.Length; i++)
            {
                content += $"{client[i].Smile} {client[i].Name}\n";

                content += $"Всего заказов: {client[i].ordersData.orders.Count}";
            }
            return content;
        }



        public static async Task update(string? apiKey, onUpdate? onUpdate, IData data, string req, string? addArg = null, onFinished? onFinished = null)
        {
            if (data == null) return;

            DateTime Msc = data.lastUpdate;
            string date = $"{Msc.Year}-{Msc.Month}-{Msc.Day}T{Msc.Hour}:{Msc.Minute}:{Msc.Second}";
            string url = $"{baseUrl}{req}?dateFrom={date}{addArg}&key={apiKey}";

            if (showUpdMessages)
                Console.WriteLine(url);

            try
            {
                string answer = new StreamReader(WebRequest.Create(url).GetResponse().GetResponseStream()).ReadToEnd();
                data.Update(answer, onUpdate);
                onFinished?.Invoke();
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

        public static void getImage(int numId)
        {
            if (File.Exists($"{output}{numId}.jpeg"))
                return;

            using (WebClient client = new WebClient())
            {
                Image? bmp1 = getBmp(client, 1);
                Image? bmp2 = getBmp(client, 2);
                Image? bmp3 = getBmp(client, 3);

                if (bmp1 != null)
                {
                    int wd = bmp1.Width;
                    int hg = bmp1.Height;

                    using (Image<Rgba32> img = new Image<Rgba32>(246 * 3, 328, Color.Black))
                    {
                        img.Mutate(c => c.DrawImage(bmp1, new Point(0, 0), 1));
                        if (bmp2 != null)
                        {
                            img.Mutate(c => c.DrawImage(bmp2, new Point(wd, 0), 1));

                            if (bmp3 != null)
                            {
                                img.Mutate(c => c.DrawImage(bmp3, new Point(wd * 2, 0), 1));
                            }
                        }

                        img.SaveAsJpeg($"{output}{numId}.jpeg");
                    }
                    bmp1?.Dispose();
                    bmp2?.Dispose();
                    bmp3?.Dispose();
                }
                else
                {
                    try
                    {
                        using (Image<Rgba32> img = new Image<Rgba32>(246 * 3, 328, Color.Black))
                        {
                            img.SaveAsJpeg($"{output}{numId}.jpeg");
                        }
                    }
                    catch (Exception e)
                    {
                        Bot.ReciveError("Cannot save image: " + numId.ToString());
                    }
                }
            }

            Image? getBmp(WebClient client, int index)
            {

                int basket = 1;

                if (numId < baskets[0])
                {
                    basket = 1;
                }
                else if (numId < baskets[1])
                {
                    basket = 2;
                }
                else if (numId < baskets[2])
                {
                    basket = 3;
                }
                else if (numId < baskets[3])
                {
                    basket = 4;
                }
                else if (numId < baskets[4])
                {
                    basket = 5;
                }
                else if (numId < baskets[5])
                {
                    basket = 6;
                }
                else if (numId < baskets[6])
                {
                    basket = 7;
                }
                else if (numId < baskets[7])
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

                        return Image.Load(client.OpenRead($"{addr}{numId / 100000}/part{numId / 1000}/{numId}/images/c246x328/{index}.jpg"));
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
            bot.clientBook[(long)query].messageCallback += (a, b, c) => Task.Run(() => GetProductPositionCallback(a, b, c));
            return ("🔎 Введите артикул товара и поисковой запрос.\n\nПример: «_68507544 медицинский костюм_»", null);
        }

        public static (string, InlineKeyboardMarkup?) GetCategoryAds(Bot bot, ClientData[]? client, object? query)
        {
            bot.clientBook[(long)query].messageCallback += (a, b, c) => Task.Run(() => GetCategoryAdsCallback(a, b, c));
            return ("🔎 Введите категорию товара, чтобы получить реальную стоимость рекламы.", null);
        }

        public static async Task GetProductPositionCallback(Bot bot, ClientData[]? client, Message? message)
        {
            string[] mes = message.Text.Split(" ");
            long id = 0;
            bot.clientBook[message.Chat.Id].messageCallback = null;

            if (mes.Length < 2 || !long.TryParse(mes[0], out id))
            {
                await bot.SendMessage(message.Chat.Id, "❌ Неверный поисковой запрос!");
                return;
            }

            string category = "";
            for (int i = 1; i < mes.Length; i++)
            {
                category += mes[i] + (i == mes.Length - 1 ? "" : " ");
            }

            try
            {
                await bot.botClient.EditMessageTextAsync(message.Chat.Id, bot.botClient.SendTextMessageAsync(message.Chat.Id, "👀 Идет поиск позиции товара...").Result.MessageId, await getCategoryItems(category, id), Telegram.Bot.Types.Enums.ParseMode.Markdown);
            }
            catch
            {
                Bot.ReciveError("Ошибка поска в категории: " + category);
            }
        }
        public static async Task GetCategoryAdsCallback(Bot bot, ClientData[]? client, Message? message)
        {
            try
            {
                string mes = message.Text;

                bot.clientBook[message.Chat.Id].messageCallback = null;
                await bot.botClient.EditMessageTextAsync(message.Chat.Id, bot.botClient.SendTextMessageAsync(message.Chat.Id, "👀 Идет поиск рекламной позиции...").Result.MessageId, getCategoryCpmList(mes), Telegram.Bot.Types.Enums.ParseMode.Markdown);
            }
            catch
            {
                Bot.ReciveError("Ошибка поиска рекламы");
            }
        }

        public static int getItemDetail(int numc)
        {
            try
            {
                string link = $"https://card.wb.ru/cards/detail?pricemarginCoeff=1.0&appType=1&locale=ru&lang=ru&curr=rub&dest=-1059500,-72639,-3826860,-5551776&nm={numc}";
                HttpContent client = new HttpClient().GetAsync(link).Result.Content;
                JObject data = JObject.Parse(new StreamReader(client.ReadAsStream()).ReadToEnd());
                //  JObject deliveryDetail = JObject.Parse(new StreamReader(client.ReadAsStream()).ReadToEnd());

                var token = data.GetValue("data")?.Value<JObject>()?.GetValue("products")?.Values<JObject>() ?? null;

                if (token != null)
                {
                    foreach (JObject obj in token)
                    {
                        int count = 0;

                        var sizes = obj?.GetValue("sizes")?.Values<JObject>() ?? null;

                        if (sizes != null)
                        {
                            foreach (var size in sizes)
                            {
                                returnCount(size.GetValue("stocks")?.Values<JObject>() ?? null);
                            }
                        }
                        else
                        {
                            returnCount(obj?.GetValue("stocks")?.Values<JObject>() ?? null);
                        }

                        void returnCount(IEnumerable<JObject?> stocks)
                        {
                            if (stocks != null)
                            {
                                foreach (JObject stock in stocks)
                                {
                                    count += stock.GetValue("qty").Value<int>();
                                }
                            }
                        }
                        return count;

                    }
                }
            }
            catch (Exception e)
            {
                Bot.ReciveError(e.Message);
                return -1;
            }
            return 0;
        }

        public static string getItemStockInfo()
        {
            HttpClient client = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://suppliers-api.wildberries.ru/api/v2/stocks?skip=0&take=3000");

            request.Headers.Add("accept", "application/json");
            request.Headers.Add("Authorization", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhY2Nlc3NJRCI6IjczODE3YWZiLWUyMTQtNDQzMS04NjEwLWE2OWFhODk5OGE1MyJ9.dsc2rw-0V8CFbsAvRiN_uv085X9msvtFRChXUaZ8p9s");

            HttpResponseMessage response = client.Send(request);
            response.EnsureSuccessStatusCode();
            return new StreamReader(response.Content.ReadAsStream()).ReadToEnd();
        }

        public static async Task<string> getCategoryItems(string category, long nmId)
        {
            Task<string> msc = GetPos(-1029256, -102269, -2162196, -1257218);
            Task<string> krs = GetPos(-1059500, -108082, -269701, 12358060);
            Task<string> omsk = GetPos(-1221148, -140292, -1558108, -3902910);

            async Task<string> GetPos(int x, int y, int w, int z)
            {
                for (int page = 0; page <= 50; page++)
                {
                    int position = 1;

                    // -1059500,-72639,-3826860,-5551775
                    //-1059500,0,-10000000,-1000000
                    //-1029256,-102269,-2162196,-1257786
                    string link = $"https://search.wb.ru/exactmatch/ru/common/v4/search?appType=1&dest={x},{y},{w},{z}&emp=0&lang=ru&locale=ru&page={page + 1}&pricemarginCoeff=1.0&reg=0&resultset=catalog&sort=popular&suppressSpellcheck=false&query={category}";
                    HttpContent client = new HttpClient().GetAsync(link).Result.Content;
                    JObject data = JObject.Parse(new StreamReader(client.ReadAsStream()).ReadToEnd());
                    var token = data.GetValue("data")?.Value<JObject>()?.GetValue("products")?.Values<JObject>() ?? null;

                    if (token != null)
                    {
                        foreach (JObject obj in token)
                        {
                            if (nmId == obj.GetValue("id").Value<long>())
                            {
                                return $"{page + 1} страница, {position} карточка";
                            }

                            position++;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                return "не ранжируется на первых 50 стр.";
            }
            await msc;
            await krs;
            await omsk;

            return $"👀 Позиция товара в поиске:\n\nℹ️ Запрос: `{nmId} {category}`\n\nМосква: {msc.Result}\nКраснодар: {krs.Result}\nОмск: {omsk.Result}";

        }

        public static string getCategoryCpmList(string category)
        {
            string link = $"https://catalog-ads.wildberries.ru/api/v5/search?keyword={category}";
            HttpContent client = new HttpClient().GetAsync(link).Result.Content;
            JObject data = JObject.Parse(new StreamReader(client.ReadAsStream()).ReadToEnd());
            var token = data.GetValue("adverts")?.Values<JObject>() ?? null;
            string content = $"🔎 Категория: {category}\n\n";

            try
            {
                int ind = 0;
                foreach (JObject obj in token)
                {
                    content += $"{ind + 1} место: {obj.GetValue("cpm").Value<int>()} руб.\n";

                    ind++;

                    if (ind >= 5)
                        break;
                }
            }
            catch
            {
                return "😢 Не удалось получить CPM в данной категории.";
            }
            return content;
        }

        public static async Task<string> GetItemName(int numId)
        {
            if (File.Exists($"{output}{numId}.txt"))
                return File.ReadAllText($"{output}{numId}.txt");
            int basket = 1;

            if (numId < baskets[0])
            {
                basket = 1;
            }
            else if (numId < baskets[1])
            {
                basket = 2;
            }
            else if (numId < baskets[2])
            {
                basket = 3;
            }
            else if (numId < baskets[3])
            {
                basket = 4;
            }
            else if (numId < baskets[4])
            {
                basket = 5;
            }
            else if (numId < baskets[5])
            {
                basket = 6;
            }
            else if (numId < baskets[6])
            {
                basket = 7;
            }
            else if (numId < baskets[7])
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
                    HttpContent client = new HttpClient().GetAsync($"{addr}{numId / 100000}/part{numId / 1000}/{numId}/info/ru/card.json").Result.Content;
                    JObject data = JObject.Parse(new StreamReader(client.ReadAsStream()).ReadToEnd());
                    string token = data.GetValue("imt_name")?.Value<string>() ?? null;

                    if (token.Length > 0)
                    {
                        File.WriteAllText($"{output}{numId}.txt", token.Replace("*", "").Replace("_", "").Replace("`", ""));
                    }
                    else
                    {
                        Console.WriteLine("Cannot read item name: " + numId.ToString());
                    }
                    return token;

                }
                catch
                {
                    if (basket >= 9)
                        return "Без названия";
                    basket++;
                }
            }
            return "Без названия";
        }

        public static bool showUpdMessages = false;
        public static async void ClientDataUpdating(object arg)
        {
            Bot bot = arg as Bot;
            while (true)
            {
                Thread.Sleep(900000);

                foreach (ClientData data in bot.clientDatas.Values)
                {
                    if (data.apiKey == null || data.balance == 0 || !data.active)
                        continue;

                    try
                    {
                        await update(data.apiKey, null, data.stocksData, "stocks");
                        await update(data.apiKey, ordersUpd, data.ordersData, "orders", "&flag=0");


                        async void ordersUpd(object ord, bool c)
                        {

                            OrdersData.Order order = ord as OrdersData.Order;

                            data.ordersData.stockCount.TryAdd(order.nmId, 0);
                            data.ordersData.stockCount[order.nmId] = WbBaseManager.getItemDetail(order.nmId);

                            if (!File.Exists($"{output}{order.nmId}.jpeg"))
                            {
                                getImage(order.nmId);
                                await GetItemName(order.nmId);
                            }

                            string content = "\n";
                            content += $"{data.Smile} {data.Name}\n";
                            content += $"_{order.lastChangeDate}_\n\n";
                            content += $"🆔 ID товара: `{order.nmId}`\n";
                            content += $"🏷 {order.Brand} | [{order.supplierArticle}](https://www.wildberries.ru/catalog/{order.nmId}/detail.aspx)\n\n";
                            content += $"📁 {order.category} | {order.techSize}\n";
                            content += $"🌐 {order.warehouseName} → {order.oblast}\n";
                            if (!order.isCancel)
                                content += $"💵 Выручка: {order.price}\n";

                            content += $"{(order.isCancel ? "🚚" : "🚛")} Статус: {(order.isCancel ? "Возврат" : "В пути")}\n";
                            content += $"\n📦 Остаток:{data.ordersData.stockCount[order.nmId]} ";

                            data.monthMessages++;
                            data.MessageRest();

                            if (!c)
                                data.dailyOrders.Add(order.odid);

                            if (File.Exists($"{output}{order.nmId}.jpeg"))
                            {
                                using (var fs = new FileStream($"{output}{order.nmId}.jpeg", FileMode.Open, FileAccess.Read))
                                {
                                    foreach (long reciver in data.recivers)
                                    {
                                        fs.Seek(0, SeekOrigin.Begin);
                                        InputOnlineFile file = new InputOnlineFile(fs);

                                        await bot.SendMessage(reciver, content, file);
                                    }
                                    fs.Close();
                                }
                            }
                            else
                            {
                                foreach (long reciver in data.recivers)
                                {
                                    await bot.SendMessage(reciver, content);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Bot.ReciveError($"Cannot read account data: {data.apiKey}. {ex.Message}");
                        continue;
                    }
                }
            }
        }

        public static async void DailyMessage(object arg)
        {
            Bot bot = arg as Bot;

            DateTime tm = DateTime.UtcNow.AddHours(3);
            tm = tm.AddHours(-tm.Hour).AddMinutes(-tm.Minute).AddSeconds(-tm.Second);

            TimeSpan waitTime = tm.AddDays(1).Subtract(DateTime.UtcNow.AddHours(3));
            while (true)
            {
                Console.WriteLine("Daily message wait time: " + waitTime.ToString());

                Thread.Sleep(waitTime);
                waitTime = new TimeSpan(1, 0, 0, 0);
                foreach (ClientData data in bot.clientDatas.Values)
                {
                    if (data.apiKey == null || data.balance == 0 || !data.active)
                        continue;

                    try
                    {
                        await dailyMess(data);

                        async Task dailyMess(ClientData data)
                        {
                            if (data.dailyOrders.Count > 0)
                            {
                                string content = $"❇️ *Статистика за {DateTime.UtcNow.ToString("d")}*\n";
                                uint ordersCount = 0;
                                uint backCount = 0;
                                int income = 0;
                                Dictionary<int, List<OrdersData.Order>> ordersDictionary = new Dictionary<int, List<OrdersData.Order>>();

                                foreach (ulong odid in data.dailyOrders)
                                {
                                    OrdersData.Order order = data.ordersData.orders[odid];

                                    if (!ordersDictionary.ContainsKey(order.nmId))
                                        ordersDictionary.Add(order.nmId, new List<OrdersData.Order>());

                                    ordersDictionary[order.nmId].Add(order);

                                    if (order.isCancel)
                                    {
                                        income -= order.price;
                                        backCount++;
                                    }
                                    else
                                    {
                                        income += order.price;
                                        ordersCount++;
                                    }

                                }

                                content += $"{data.Smile} {data.Name}\n\n";
                                content += $"🚛 Заказы: {ordersCount}\n";
                                content += $"🚚 Возвраты: {backCount}\n";
                                content += $"💰 Выручка: {income}\n\n";

                                content += "Ⓜ️ *Самые популярные товары:*\n";

                                KeyValuePair<int, List<OrdersData.Order>>[] popular = ordersDictionary.OrderBy(x => x.Value.Count).Take(ordersDictionary.Count >= 2 ? 2 : ordersDictionary.Count).ToArray();

                                foreach (KeyValuePair<int, List<OrdersData.Order>> pop in popular)
                                {
                                    content += $"\n📘 Товар:{pop.Value[0].itemName}\n";
                                    int c = 0;
                                    int b = 0;
                                    float sm = 0;
                                    foreach (OrdersData.Order ord in pop.Value)
                                    {
                                        if (ord.isCancel)
                                            b++;
                                        else
                                        {
                                            sm += ord.price;
                                            c++;
                                        }
                                    }
                                    content += $"🚛 Заказы: {c}\n";
                                    content += $"🚚 Возвраты: {b}\n";
                                    content += $"📦 Остаток:{data.ordersData.stockCount[pop.Key]}\n";
                                    content += $"💰 Выручка: {sm}\n";


                                }
                                KeyValuePair<int, int>[] ending = data.ordersData.stockCount.OrderBy(x => x.Value).Take(data.ordersData.stockCount.Count >= 2 ? 2 : data.ordersData.stockCount.Count).ToArray();

                                content += "\n⚠️ *Товары на исходе:*\n";

                                foreach (KeyValuePair<int, int> end in ending)
                                {
                                    OrdersData.Order cr = data.ordersData.orders[data.ordersData.uniqOrders[end.Key][0]];
                                    content += $"\n📘 Товар:{cr.itemName}\n";
                                    int c = 0;
                                    int b = 0;
                                    float sm = 0;
                                    foreach (OrdersData.Order ord in ordersDictionary[end.Key])
                                    {
                                        if (ord.isCancel)
                                            b++;
                                        else
                                        {
                                            sm += ord.price;
                                            c++;
                                        }
                                    }
                                    content += $"🚛 Заказы: {c}\n";
                                    content += $"🚚 Возвраты: {b}\n";
                                    content += $"📦 Остаток:{end.Value}\n";
                                    content += $"💰 Выручка: {sm}\n";
                                }

                                content += $"Дата последнего заказа: {data.ordersData.orders[data.dailyOrders[^1]].date}";

                                data.dailyOrders.Clear();
                                foreach (long id in data.recivers)
                                {
                                    Task<Message> mes = new Task<Message>(() => bot.SendMessage(id, content).Result);
                                    mes.Start();
                                    await bot.botClient.PinChatMessageAsync(id, mes.Result.MessageId);
                                }
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        Bot.ReciveError(e.Message);
                    }
                }
            }
        }

        public static (string, InlineKeyboardMarkup?) ProductInfo(Bot bot, ClientData[]? client, object? arg){
            long senderId = (long)arg;


            bot.clientBook[senderId].messageCallback = SendProductInfo;

            return ("🔎 Введите ID товара:", null);
        }

        public static async void SendProductInfo(Bot bot, ClientData[]? client, Message? message){
            try{
                int art = int.Parse(message?.Text?? throw new Exception("message null"));

                int stockInfo = getItemDetail(art);
                string name = await WbBaseManager.GetItemName(art);

                await bot.SendMessage(message.Chat.Id, $"📘 Товар: {name}\n\n🆔 ID товара: `{art}`\n📦 Остаток: {stockInfo}\n\n⚠️ Данная функция была добавлена недавно и еще неоднократно получит обновление.");
                bot.clientBook[message.Chat.Id].messageCallback = null;
            }catch(Exception e){
                await bot.SendMessage(message.Chat.Id, "❌ ID товара введен некорректно. Проверьте правильность написания и повторите попытку.");
                Bot.ReciveError("Cannot Get product Info " + e.Message);
            }
        }


        public static (string, InlineKeyboardMarkup?) ClientDataOrders(Bot bot, ClientData[]? client, object? arg)
        {
            long senderId = (long)arg;

            if (client.Length == 1)
            {
                bot.clientBook[senderId].queryCallback = OrderStatistic;
                return (OrderStatistic(bot, client, "/products 0 0"));
            }
            else if (client.Length > 1)
            {
                bot.clientBook[senderId].queryCallback = OrderStatistic;

                InlineKeyboardButton[][] buttons = new InlineKeyboardButton[client.Length][];

                for (int i = 0; i < buttons.Length; i++)
                {
                    buttons[i] = new InlineKeyboardButton[] { new InlineKeyboardButton(client[i].Name) { CallbackData = $"/products {i} 0" } };
                }
                bot.clientBook[senderId].queryCallback = OrderStatistic;
                return ("⬇️ Выберите нужный аккаунт для просмотра товаров:", buttons);
            }

            return ("❌ У вас нет ни одного привязанного аккаунта!", null);
        }

        public static (string, InlineKeyboardMarkup?) OrderStatistic(Bot bot, ClientData[]? client, object? arg)
        {
            try
            {
                string[] args = arg is string ? ((string)arg).Split(' ') : ((CallbackQuery)arg).Data.Split(' ');
                ClientData handleClient = client[int.Parse(args[1])];

                InlineKeyboardButton backButton = new InlineKeyboardButton("Назад") { CallbackData = "/products back" };

                if (args[1] == "back")
                {
                    return ClientDataOrders(bot, client, ((CallbackQuery)arg).Message.Chat.Id);
                }

                if (handleClient.ordersData.uniqOrders.Count == 0)
                {
                    return ("❌ На выбранном аккаунте не найден ни один товар. Скорее всего вы совсем недавно начали бользоваться нашим ботом, либо к вам не поступали заказы в течении последних трех месяцев.\nНичего, скоро все изменится!", client.Length > 1 ? backButton : null);
                }

                int ord = int.Parse(args[2]);
                DateTime dt = DateTime.UtcNow.AddHours(3).AddDays(-7);
                string content = $"⬇️ *Статистика за неделю (с {dt.Day}.{dt.Month}.{dt.Year})*\n _Страница {ord / 2 + 1}/{(handleClient.ordersData.uniqOrders.Count + 1) / 2}_\n\n";
                List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();

                for (int i = ord; i < Math.Min(ord + 2, handleClient.ordersData.uniqOrders.Count); i++)
                {
                    List<ulong> order = handleClient.ordersData.uniqOrders.ElementAt(i).Value;
                    int ordCount = 0;
                    int backCount = 0;
                    double income = 0;

                    for (int j = 1; j <= order.Count; j++)
                    {
                        OrdersData.Order o = handleClient.ordersData.orders[order[^j]];

                        if (o.date > dt)
                        {
                            if (o.isCancel)
                            {
                                backCount++;
                            }
                            else
                            {
                                ordCount++;
                                income += o.price;
                            }
                        }
                    }

                    OrdersData.Order curOrd = handleClient.ordersData.orders[order[^1]];
                    content += $"*{curOrd.itemName}*\n";
                    content += $"🆔 ID товара: `{curOrd.nmId}`\n";
                    content += $"🏷 {curOrd.Brand} | [{curOrd.supplierArticle}](https://www.wildberries.ru/catalog/{curOrd.nmId}/detail.aspx)\n";
                    content += $"📁 {curOrd.category} | {curOrd.techSize}\n";
                    content += $"🚛 Заказы: {ordCount}\n";
                    content += $"🚚 Возвраты: {backCount}\n";
                    content += $"📦 Остаток: {handleClient.ordersData.stockCount[curOrd.nmId]}\n";
                    content += $"💰 Выручка: {income}\n\n";
                }


                if (ord > 0 || ord < handleClient.ordersData.uniqOrders.Count - 2)
                {
                    if (ord == 0)
                    {
                        buttons.Add(new InlineKeyboardButton[]
                        {
                        new InlineKeyboardButton("След →"){ CallbackData = $"/products {args[1]} {ord +2}" },
                        });
                    }
                    else if (ord >= handleClient.ordersData.uniqOrders.Count - 2)
                    {
                        buttons.Add(new InlineKeyboardButton[]
                        {
                        new InlineKeyboardButton("← Пред"){ CallbackData = $"/products {args[1]} {ord -2}" },
                        });
                    }
                    else
                    {
                        buttons.Add(new InlineKeyboardButton[]
                        {
                        new InlineKeyboardButton("← Пред"){ CallbackData = $"/products {args[1]} {ord -2}" },
                        new InlineKeyboardButton("След →"){ CallbackData = $"/products {args[1]} {ord +2}" },
                        });
                    }
                }

                if (client.Length > 1)
                    buttons.Add(new InlineKeyboardButton[] { backButton });

                return (content, buttons.Count > 0 ? buttons.ToArray() : null);
            }
            catch (Exception e)
            {
                Bot.ReciveError(e.Message);
            }
            return ("Системная ошибка. Мы уже работаем над ее исправлением", null);
        }


    }
}
