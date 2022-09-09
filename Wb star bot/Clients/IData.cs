using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wb_star_bot.Clients
{
    internal interface IData
    {
        DateTime lastUpdate { get; set; }

        DateTime lastMessage { get; }

        void Update(string content);

        string? GetContent(DateTime startDate, DateTime? endDate = null);
    }
}
