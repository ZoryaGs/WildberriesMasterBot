using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wb_star_bot.Clients
{
    internal class ClientData
    {
        public string Name { get; set; }
        public string? apiKey = null;

        public IncomeData incomeData;
        public OrdersData ordersData;
        public SalesData salesData;
        public StocksData stocksData;

        public List<long> recivers = new List<long>();

        public ClientData(string api, params long[] recivers)
        {
            Name = "unnamed";
            apiKey = api;
            this.recivers.AddRange(recivers);

            incomeData = new IncomeData();
            ordersData = new OrdersData();
            salesData = new SalesData();
            stocksData = new StocksData();
        }
    }
}
