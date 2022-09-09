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
        }
        public string? GetContent(DateTime startDate, DateTime? endDate = null)
        {
            if (orders.Count > 0)
            {
                string content = "";
                List<Order> newOrders = new List<Order>();

                for (int i = 1; i <= orders.Count; i++)
                {
                    Order order = orders.ElementAt(^i).Value;

                    if (endDate != null)
                        if (order.lastChangeDate > endDate)
                            continue;

                    if (startDate > order.lastChangeDate)
                        break;

                    newOrders.Add(order);
                }

                foreach(Order order in newOrders)
                {
                    content += $"🆔 Артикул WB: {order.nmId}\n";
                    content += $"📁 {order.category} | {order.techSize}\n";
                    content += $"{(order.isCancel ? "🚚" : "🚛")} Статус: {(order.isCancel ? "Возврат" : "В пути")}\n";
                }

                return content.Length > 0 ? content : null;
            }

            return null;
        }

        public async Task sendNewOrders(Bot bot, ClientData data, DateTime startDate, DateTime? endDate = null)
        {
            if (orders.Count > 0)
            {
                List<Order> newOrders = new List<Order>();

                for (int i = 1; i <= orders.Count; i++)
                {
                    Order order = orders.ElementAt(^i).Value;

                    if (endDate != null)
                        if (order.lastChangeDate > endDate)
                            continue;

                    if (startDate > order.lastChangeDate)
                        break;

                    newOrders.Add(order);
                }

                string outPut = $"{Directory.GetCurrentDirectory()}/";

                foreach (Order order in newOrders)
                {
                    string content = "";

                    if (!File.Exists($"{outPut}{order.nmId}.jpeg"))
                    {
                        WbBaseManager.getImage(order.nmId, outPut);
                    }
                    content += $"🍒 {data.Name}\n";
                    content += $"🆔 Артикул WB: {order.nmId}\n";
                    content += $"📁 {order.category} | {order.techSize}\n";
                    content += $"{(order.isCancel ? "🚚" : "🚛")} Статус: {(order.isCancel ? "Возврат" : "В пути")}\n";

                    using (var fs = new FileStream($"{outPut}{order.nmId}.jpeg", FileMode.Open, FileAccess.Read)) {
                        foreach (long reciver in data.recivers) {
                            await bot.SendMessage(reciver, content, fs);
                        }
                    }
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
        }
    }
}
