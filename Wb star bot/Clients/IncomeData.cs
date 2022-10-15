using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Wb_star_bot.Clients
{
    [Serializable]
    internal class IncomeData : IData
    {
        public DateTime lastUpdate { get; set; } = new DateTime(2017, 1, 1, 0, 0, 0);

        public DateTime lastMessage
        {
            get
            {
                if (incomes.Count == 0)
                    return lastUpdate;

                return incomes.ElementAt(^1).Value[0].lastChangeDate;
            }
        }

        public Dictionary<int, List<Income>> incomes = new Dictionary<int, List<Income>>();

        public void Update(string content, ClientData parentData)
        {
            lastUpdate = DateTime.UtcNow;

            Income[] newIncomes = JsonConvert.DeserializeObject<Income[]>(content) ?? new Income[0];

            foreach (var income in newIncomes)
            {
                if (incomes.ContainsKey(income.incomeid))
                {
                    if (income.status != incomes[income.incomeid][0].status)
                    {
                        incomes[income.incomeid].Clear();
                    }
                    incomes[income.incomeid].Add(income);
                }
                else
                {
                    incomes.Add(income.incomeid, new List<Income>() { income });
                }
            }
        }

        public string? GetContent(DateTime startDate, DateTime? endDate = null)
        {
            if (incomes.Count > 0)
            {
                string content = "";

                for (int i = 1; i <= incomes.Count; i++)
                {
                    List<Income> incomeList = incomes.ElementAt(^i).Value;
                    Income zeroElemet = incomeList[0];

                    if (endDate != null)
                        if (zeroElemet.lastChangeDate > endDate)
                            continue;

                    if (startDate > zeroElemet.lastChangeDate)
                        break;

                    content += $"Номер поставки: {zeroElemet.incomeid}, время {zeroElemet.lastChangeDate}, статус {zeroElemet.status}\n";

                    foreach (Income income in incomeList)
                    {
                        content += $"Размер: {income.techSize}, количесвто {income.quantity}\n";
                    }

                }

                return content.Length > 0 ? content : null;
            }
            return null;
        }

        public class Income
        {
            public int incomeid;
            public string number;
            public DateTime date;
            public DateTime lastChangeDate;
            public string supplierArticle;
            public string techSize;
            public string barcode;
            public int quantity;
            public int totalPrice;
            public DateTime dateClose;
            public string warehouseName;
            public int nmId;
            public string status;
        }
    }
}
