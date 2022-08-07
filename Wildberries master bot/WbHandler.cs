using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Xml;

namespace Wildberries_master_bot
{
    internal static class WbHandler
    {
        public static string IncomeToday(ClientData data) => request(data, new DateTime(2022, 8, 7,0,0,0), "incomes");
        public static string IncomeMounth(ClientData data) => request(data, new DateTime(2022, 7, 1,0,0,0), "incomes");

        private static string request(ClientData data, DateTime dateTime, string req)
        {
            string url = $"https://suppliers-stats.wildberries.ru/api/v1/supplier/{req}?dateFrom={XmlConvert.ToString(dateTime, XmlDateTimeSerializationMode.Utc)}&key={data.apiKey}";
            string answer = new StreamReader(WebRequest.Create(url).GetResponse().GetResponseStream()).ReadToEnd();
            if(answer.Length > 1024)
            {
                answer = answer.Remove(1024);
            }
            return answer;
        }
    }
}
