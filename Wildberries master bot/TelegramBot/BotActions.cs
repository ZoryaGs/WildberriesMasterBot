using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildberries_master_bot.TelegramBot
{
    internal class BotAction
    {
        public string command;
        public result action;

        public delegate string result(ClientData data);

        public BotAction(string command, result action)
        {
            this.command = command;
            this.action = action;
        }
    }

}
