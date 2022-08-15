using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Wildberries_master_bot.DataTypes;

namespace Wildberries_master_bot.TelegramBot
{
    public delegate void messageReciver(ITelegramBotClient botClient, Update update);

    [Serializable]
    internal class ClientData
    {
        public bool paid => paidPeriod > DateTime.UtcNow;

        private DateTime paidPeriod = DateTime.MinValue;

        public string? apiKey = null;

        [NonSerialized]
        public Dictionary<string, IData> DataBase;

        public IncomeData IncomeData;
        public StocksData StocksData;
        public OrdersData OrdersData;

        [NonSerialized]
        public messageReciver? messageReciver;

        public ClientData()
        {
            IncomeData = new IncomeData();
            StocksData = new StocksData();
            OrdersData = new OrdersData();

            DataBase = new Dictionary<string, IData>()
            {
                {"/income", IncomeData },
                {"/supplies", StocksData },
                {"/orders", OrdersData },
            };
            
        }

        public string GetData(string key)
        {
            IData data = DataBase[key];
            switch (key)
            {
                case "/income":

                    break;
                case "/orders":
                    messageReciver = reciveOrders;
                    return $"Всего доставок: {((OrdersData)data).orders.Count}";
                    break;
                case "/supplies":

                    break;
            }
            return "error";
        }

        public void reciveOrders(ITelegramBotClient botClient, Update update)
        {

        }
    }
}
