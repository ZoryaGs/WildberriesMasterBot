using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildberries_master_bot.TelegramBot
{
    internal class BotPage
    {
        public string PageMessage;
        public BotAction[] Actions = null;

        public BotPage(string PageMessage, BotAction[] Actions)
        {
            this.PageMessage = PageMessage;
            this.Actions = Actions;
        }
        public BotPage(string PageMessage)
        {
            this.PageMessage = PageMessage;
        }
    }
}
