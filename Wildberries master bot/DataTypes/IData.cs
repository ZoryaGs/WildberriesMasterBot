using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildberries_master_bot.DataTypes
{
    internal interface IData
    {
        DateTime lastUpdate { get; set; }

        DateTime lastMessage { get; }

        void Update(string content);

        string? GetContent(DateTime lastTime);
    }
}
