using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wb_star_bot.Telegram_Bot;

namespace Wb_star_bot.Clients
{
    internal class Client
    {
        public List<string> clientDatas = new List<string>();
        public QueryCallback? queryCallback;
        public MessageCallback? messageCallback;

        public Client(params string[] clientDatas)
        {
            this.clientDatas.AddRange(clientDatas);
        }

    }
}
