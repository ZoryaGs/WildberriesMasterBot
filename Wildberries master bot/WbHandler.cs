using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Xml;
using Wildberries_master_bot.TelegramBot;
using Wildberries_master_bot.DataTypes;

#pragma warning disable SYSLIB0014 

namespace Wildberries_master_bot
{
    internal static class WbHandler
    {
        private const string baseUrl = "https://suppliers-stats.wildberries.ru/api/v1/supplier/";
        private static DateTime timeNow => DateTime.UtcNow;

        private static DateTime currentDay => new DateTime(timeNow.Year, timeNow.Month, timeNow.Day);
        private static DateTime currentMounth => new DateTime(timeNow.Year, timeNow.Month, 1);
        private static DateTime lastDay => new DateTime(timeNow.Year, timeNow.Month, timeNow.Day).AddDays(-1);
        private static DateTime lastMount => new DateTime(timeNow.Year, timeNow.Month, 1).AddMonths(-1);

        public static string IncomeToday(ClientData data) => request(data, data.incomeData, currentDay, "incomes", "Поставок за сегодня нет");
        public static string IncomeMounth(ClientData data) => request(data, data.incomeData, currentMounth, "incomes", "Поставок за текущий месяц нет");
        public static string IncomeLastToday(ClientData data) => request(data, data.incomeData, lastDay, "incomes", "Поставок за вчера нет");
        public static string IncomeLastMounth(ClientData data) => request(data, data.incomeData, lastMount, "incomes", "Поставок за прошлый месяц нет");

        public static string OrdersToday(ClientData data) => request(data, data.ordersData, currentDay, "orders", "Заказов за сегодня нет", "&flag=0");
        public static string OrdersMounth(ClientData data) => request(data, data.ordersData, currentMounth, "orders", "Заказов за текущий месяц нет", "&flag=0");
        public static string OrdersLastToday(ClientData data) => request(data, data.ordersData, lastDay, "orders", "Заказов за вчера нет", "&flag=0");
        public static string OrdersLastMounth(ClientData data) => request(data, data.ordersData, lastMount, "orders", "Заказов за прошлый месяц нет", "&flag=0");

        private static string AllIncome(ClientData data) => request(data, data.incomeData, new DateTime(2017, 1, 1, 0, 0, 0), "incomes", "");
        private static string AllOrders(ClientData data) => request(data, data.ordersData, new DateTime(2017, 1, 1, 0, 0, 0), "orders", "", "&flag=0");

        private static string request(ClientData client, IData data, DateTime dateTime, string req, string errText, string? addArg = null)
        {
            if (data.lastUpdate == DateTime.MinValue || timeNow.Subtract(data.lastUpdate).TotalMinutes > 10)
            {
                string date = XmlConvert.ToString(data.lastMessage, XmlDateTimeSerializationMode.Utc);
                string url = $"{baseUrl}{req}?dateFrom={date}{addArg}&key={client.apiKey}";
                Console.WriteLine($"Update client {url}");
                try
                {
                    string answer = new StreamReader(WebRequest.Create(url).GetResponse().GetResponseStream()).ReadToEnd();
                    Console.WriteLine(answer);
                    data.Update(answer);
                }
                catch(WebException e)
                {
                    Console.Write(e.GetBaseException().ge);
                }
            }
            return data.GetContent(dateTime) ?? errText;
        }

        public static async Task ClientInit(ClientData data)
        {
            string income = AllIncome(data);
            string orders = AllOrders(data);

            if (income.Length == 0 || orders.Length == 0)
            {
                Console.WriteLine($"Клиент не инициализирован! Ключ клиента: {data.apiKey}");
            }
        }
    }
}
