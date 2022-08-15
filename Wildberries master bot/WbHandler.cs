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
        private static DateTime lastMounth => new DateTime(timeNow.Year, timeNow.Month, 1).AddMonths(-1);

        public static string IncomeToday(ClientData data) => data.DataBase["/income"].GetContent(currentDay) ?? "Поставок за сегодня нет";
        public static string IncomeMounth(ClientData data) => data.DataBase["/income"].GetContent(currentMounth) ?? "Поставок за текущий месяц нет";
        public static string IncomeLastToday(ClientData data) => data.DataBase["/income"].GetContent(lastDay) ?? "Поставок за вчера нет";
        public static string IncomeLastMounth(ClientData data) => data.DataBase["/income"].GetContent(lastMounth) ?? "Поставок за прошлый месяц нет";

        public static string OrdersToday(ClientData data) => data.DataBase["/orders"].GetContent(currentDay) ?? "Заказов за сегодня нет";
        public static string OrdersMounth(ClientData data) => data.DataBase["/orders"].GetContent(currentMounth) ?? "Заказов за текущий месяц нет";
        public static string OrdersLastToday(ClientData data) => data.DataBase["/orders"].GetContent(lastDay) ?? "Заказов за вчера нет";
        public static string OrdersLastMounth(ClientData data) => data.DataBase["/orders"].GetContent(lastMounth) ?? "Заказов за прошлый месяц нет";


        private static void update(string apiKey, IData data, string req, string? addArg = null)
        {
            string url = $"{baseUrl}{req}?dateFrom={XmlConvert.ToString(data.lastMessage, XmlDateTimeSerializationMode.Utc)}{addArg}&key={apiKey}";
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

        public static async Task ClientDataUpdateAsync(ClientData data)
        {
            if (data.apiKey != null)
            {
                update(data.apiKey, data.DataBase["/income"], "incomes");
                update(data.apiKey, data.DataBase["/orders"], "orders", "&flag=0");
            }

        }
        public static void ClientDataUpdate(ClientData data)
        {
            if (data.apiKey != null)
            {
                update(data.apiKey, data.DataBase["/income"], "incomes");
                update(data.apiKey, data.DataBase["/orders"], "orders", "&flag=0");
            }
        }
    }
}
