using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Wildberries_master_bot.DataTypes;

namespace Wildberries_master_bot.TelegramBot
{
    [Serializable]
    internal class ClientData 
    {
        public bool paid => paidPeriod > DateTime.UtcNow;

        private DateTime paidPeriod = DateTime.MinValue;

        public int loadedPage = 0;
        public string apiKey = "";

        public IncomeData incomeData;
        public StocksData suppliesData;
        public OrdersData ordersData;

        public ClientData(int loadedPage)
        {
            this.loadedPage = loadedPage;

            incomeData = new IncomeData();
            suppliesData = new StocksData();
            ordersData = new OrdersData();
        }
    }
}
