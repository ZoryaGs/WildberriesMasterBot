using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wb_star_bot.Wb_handler;
using Newtonsoft.Json;

namespace Wb_star_bot.Clients
{
    [Serializable]
    internal class SalesData : IData
    {
        public DateTime lastUpdate { get; set; } = new DateTime(2017, 1, 1, 0, 0, 0);

        public DateTime lastMessage { get => lastUpdate; }

        public Dictionary<long, Sale> sales = new Dictionary<long, Sale>();

        public void Update(string content, ClientData parentData)
        {
            lastUpdate = DateTime.UtcNow;

            Sale[] newSales = JsonConvert.DeserializeObject<Sale[]>(content) ?? new Sale[0];

            foreach (var sale in newSales)
            {
                if (!sales.ContainsKey(sale.odid))
                {
                    sales.Add(sale.odid, sale);
                }
            }
        }
        public string? GetContent(DateTime startDate, DateTime? endDate = null)
        {
            if (sales.Count > 0)
            {
                string content = "";

                for (int i = 1; i <= sales.Count; i++)
                {
                    Sale sale = sales.ElementAt(^i).Value;

                    if (endDate != null)
                        if (sale.lastChangeDate > endDate)
                            continue;

                    if (startDate > sale.lastChangeDate)
                        break;

                    content += $"{sale.subject} ({sale.techSize})\n{sale.date} {sale.odid}\n";
                }
                return content.Length > 0 ? content : null;
            }
            return null;
        }

        public class Sale
        {
            public DateTime date;
            public DateTime lastChangeDate;
            public string supplierArticle;
            public string techSize;
            public string? barcode;
            public float totalPrice;
            public int discountPercent;
            public string warehouseName;
            public string oblast;
            public int? incomeID;
            public long odid;
            public int nmId;
            public string subject;
            public string category;
            public bool isCancel;
            public string gNumber;

            public string status => isCancel ? "❌" : "✅";
        }
    }
}
