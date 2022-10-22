using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wb_star_bot.Clients
{
    internal class ClientData
    {
        public const int standartTarifCost = 190;
        public const int premiumTarifCost = 490;
        public string Name { get => name; set { name = value.Replace("_","").Replace("`", "").Replace("*",""); } }
        public string Smile { get => active ? smile : "❌"; set { smile = value; } }

        private string smile = "";
        private string name = "";

        public string? apiKey = null;
        public string? apiSecret = null;

        public OrdersData ordersData;
        public StocksData stocksData;

        public List<ulong> dailyOrders = new List<ulong>();

        public List<long> recivers = new List<long>();

        public int monthMessages = 0;

        public DateTime lastPaid;
        public double balance = 0;
        public string? promocode = null;

        public subscibeType tarif = subscibeType.none;

        public bool active = false;

        public enum subscibeType
        {
            none,
            simple,
            premium,
        }

        public ClientData(string api, params long[] recivers)
        {
            Name = "unnamed";
            apiKey = api;
            this.recivers.AddRange(recivers);

            ordersData = new OrdersData();
            stocksData = new StocksData();
        }


        public bool AddBalance(double number)
        {
            lastPaid = DateTime.UtcNow;
            balance += number;

            if (active == false)
            {
                active = true;
                return false;
            }

            return true;
        }

        public bool SelectTarif(subscibeType tarif)
        {
            int cost = TarifCost(tarif);
            if (balance >= cost)
            {
                BalanceRest(cost);
                this.tarif = tarif;
                return true;
            }
            return false;
        }

        public int TarifCost(subscibeType tarif)
        {
            switch (tarif)
            {
                case subscibeType.simple:
                    if (this.tarif == subscibeType.none) return standartTarifCost; 
                    break;
                case subscibeType.premium:
                    if (this.tarif == subscibeType.none) return premiumTarifCost;
                    if (this.tarif == subscibeType.simple) return premiumTarifCost - standartTarifCost;
                    break;
            }
            return 0;
        }

        public void MessageRest()
        {
            if (monthMessages < 10000)
            {
                BalanceRest(0.03);
            }
            else
            {
                BalanceRest(0.005);
            }
        }

        public void BalanceRest(double cost)
        {
            balance -= cost;

            if (balance <= 0)
            {
                balance = 0;
                active = false;
            }
        }
    }
}
