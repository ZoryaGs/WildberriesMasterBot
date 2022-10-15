using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wb_star_bot.Telegram_Bot;
using System.IO;
using Wb_star_bot.Wb_handler;
using Newtonsoft.Json;

namespace Wb_star_bot.Clients
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
        public Dictionary<int, Order> uniqOrders = new Dictionary<int, Order>();

        public void Update(string content, onUpdate? onUpdate)
        {
            lastUpdate = DateTime.UtcNow;

            Order[] newOrders = JsonConvert.DeserializeObject<Order[]>(content) ?? new Order[0];

            foreach (var order in newOrders)
            {
                if (!orders.TryAdd(order.odid, order))
                {
                    orders[order.odid] = order;
                }
                else
                {
                    onUpdate?.Invoke(this, false);
                }
                if (uniqOrders.ContainsKey(order.nmId))
                {
                    uniqOrders[order.nmId] = order;
                }
                else {
                    uniqOrders.Add(order.nmId, order);
                }
            }
        }

        public class Order
        {
            public DateTime date;
            public DateTime lastChangeDate;
            public string supplierArticle;
            public string techSize;
            public string barcode;
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

            [NonSerialized]
            public int count;

            public string itemName
            {
                get
                {
                    string output = Directory.GetCurrentDirectory() + "/" + nmId.ToString() + ".txt";
                    if (File.Exists(output)){
                        return File.ReadAllText(output);
                    }
                    return "Без названия";
                }
            }
        }
    }
}
