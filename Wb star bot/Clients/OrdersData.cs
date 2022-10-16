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
        public Dictionary<int, List<ulong>> uniqOrders = new Dictionary<int, List<ulong>>();


        public void Update(string content, onUpdate? onUpdate)
        {

            Order[] newOrders = JsonConvert.DeserializeObject<Order[]>(content) ?? new Order[0];
            foreach (var order in newOrders)
            {
                if (!orders.TryAdd(order.odid, order))
                {
                    if (orders[order.odid].isCancel != order.isCancel)
                    {
                        onUpdate?.Invoke(order, true);
                    }

                    orders[order.odid] = order;
                }
                else
                {
                    if (uniqOrders.ContainsKey(order.nmId))
                    {
                        for (int i = uniqOrders[order.nmId].Count - 1; i >= 0; i--)
                        {
                            if (uniqOrders[order.nmId][i] < order.odid)
                            {
                                uniqOrders[order.nmId].Insert(i, order.odid);
                                break;
                            }
                        }
                    }
                    else
                    {
                        uniqOrders.Add(order.nmId, new List<ulong>() { order.odid });
                    }

                    onUpdate?.Invoke(order, false);
                }

            }

            if (newOrders.Length > 0)
                lastUpdate = newOrders[^1].lastChangeDate;
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
            public string brand;
            public string subject;
            public string category;
            public bool isCancel;
            public string gNumber;

            [NonSerialized]
            public int count;

            public int price
            {
                get
                {
                    return (int)Math.Floor(totalPrice * (1f - discountPercent / 100f));
                }
            }

            public string itemName
            {
                get
                {
                    string output = Directory.GetCurrentDirectory() + "/" + nmId.ToString() + ".txt";
                    if (File.Exists(output)){
                        return File.ReadAllText(output);
                    }
                    return "_имя еще не получено_";
                }
            }
        }
    }
}
