using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildberries_master_bot.DataTypes
{
    [Serializable]
    internal class StocksData : IData
    {
        public DateTime lastUpdate { get; set; } = new DateTime(2017, 1, 1, 0, 0, 0);

        public DateTime lastMessage { get; }

        public void Update(string content)
        {
            lastUpdate = DateTime.UtcNow;
        }
        public string? GetContent(DateTime dateTime)
        {
            return "";
        }
    }
}
