using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Wildberries_master_bot.DataTypes
{
    [Serializable]
    internal class OrdersData : IData
    {
        public DateTime lastUpdate { get; set; } = new DateTime(2017, 1, 1, 0, 0, 0);
        public DateTime lastMessage
        {
            get
            {
                if (orders.Count == 0)
                    return lastUpdate;

                return orders.ElementAt(^1).Value.lastChangeDate;
            }
        }

        public Dictionary<ulong, Order> orders = new Dictionary<ulong, Order>();

        public void Update(string content)
        {
            lastUpdate = DateTime.UtcNow;

            Order[] newOrders = JsonConvert.DeserializeObject<Order[]>(content) ?? new Order[0];

            foreach (var order in newOrders)
            {
                if (!orders.TryAdd(order.odid, order))
                {
                    orders[order.odid] = order;
                }
            }
            Console.WriteLine(orders.Count);
        }
        public string? GetContent(DateTime dateTime)
        {
            if (orders.Count > 0)
            {
                string content = "";

                for (int i = 1; i <= orders.Count; i++)
                {
                    Order order = orders.ElementAt(^i).Value;

                    if (dateTime > order.date)
                        break;
                    content += $"{order.odid}\n";
                }
                return content;
            }

            return null;
        }


        public class Order
        {
            public DateTime date;
            public DateTime lastChangeDate;
            public string supplierArticle;
            public string techSize;
            public ulong barcode;
            public float totalPrice;
            public int discountPercent;
            public string warehouseName;
            public string oblast;
            public int incomeID;
            public ulong odid;
            public int nmId;
            public string subject;
            public string category;
            public bool isCancel;
            public string gNumber;
        }
    }
}
