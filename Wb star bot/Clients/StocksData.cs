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

        public void Update(string content, onUpdate? onUpdate)
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
                    onUpdate?.Invoke(stock, true);
                }
                else
                {
                    stocks.Add(stock.warehouse, new List<Stock>() { stock });
                    onUpdate?.Invoke(stock, false);
                }
            }
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
