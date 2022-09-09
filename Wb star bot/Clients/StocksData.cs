using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Wb_star_bot.Clients
{
    [Serializable]
    internal class StocksData : IData
    {
        public DateTime lastUpdate { get; set; } = new DateTime(2017, 1, 1, 0, 0, 0);

        public DateTime lastMessage { get => lastUpdate; }

        public Dictionary<int, List<Stock>> stocks = new Dictionary<int, List<Stock>>();

        public void Update(string content)
        {
            Stock[] ldStocks = JsonConvert.DeserializeObject<Stock[]>(content) ?? new Stock[0];

            if (ldStocks.Length == 0)
                return;

            stocks.Clear();

            foreach (var stock in ldStocks)
            {
                if (stocks.ContainsKey(stock.warehouse))
                {
                    stocks[stock.warehouse].Add(stock);
                }
                else
                {
                    stocks.Add(stock.warehouse, new List<Stock>() { stock });
                }
            }
        }
        public string? GetContent(DateTime startDate, DateTime? endDate = null)
        {
            if (stocks.Count > 0)
            {
                string content = "";
                int wayToClient = 0;
                int backFromClient = 0;
                int quantitytNotInOrders = 0;

                for (int i = 1; i <= stocks.Count; i++)
                {
                    List<Stock> stockList = stocks.ElementAt(^i).Value;
                    Stock zeroElemet = stockList[0];

                    content += $"Номер склада: {zeroElemet.warehouse}, место: {zeroElemet.warehouseName}\n";

                    foreach (Stock stock in stockList)
                    {
                        quantitytNotInOrders += stock.quantityNotInOrders;
                        backFromClient += stock.inWayFromClient;
                        wayToClient += stock.inWayToClient;
                    }
                    content += $"📦 Остатки всего: {quantitytNotInOrders}\n";
                    content += $"✈️ Заказы в пути: {wayToClient}\n";
                    content += $"🚚 Возвраты в пути: {backFromClient}\n";

                }
                return content.Length > 0 ? content : null;
            }

            return null;
        }

        public class Stock
        {
            public DateTime lastChangeDate;
            public string supplierArticle;
            public string techSize;
            public string barcode;
            public int quantity;
            public bool isSupply;
            public bool isRealization;
            public int quantityFull;
            public int quantityNotInOrders;
            public int warehouse;
            public string warehouseName;
            public int inWayToClient;
            public int inWayFromClient;
            public int nmId;
            public string subject;
            public string category;
            public int daysOnSite;
            public string SCCode;
            public int Price;
            public int Discount;

        }
    }
}
