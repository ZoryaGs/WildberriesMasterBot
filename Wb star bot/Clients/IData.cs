using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wb_star_bot.Clients
{
    internal delegate void onUpdate(object data, bool containts);

    internal interface IData
    {
        DateTime lastUpdate { get; set; }

        DateTime lastMessage { get; }

        void Update(string content, onUpdate? onUpdate);
    }
}
