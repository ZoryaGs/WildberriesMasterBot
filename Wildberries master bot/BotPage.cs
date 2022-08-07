using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildberries_master_bot
{
    internal class BotPage
    {
        public string PageCommand;
        public string PageMessage;
        public BotAction[] Actions = null;

        public BotPage(string PageCommand, string PageMessage, BotAction[] Actions)
        {
            this.PageCommand = PageCommand;
            this.PageMessage = PageMessage;
            this.Actions = Actions;
        }
        public BotPage(string PageCommand, string PageMessage)
        {
            this.PageCommand = PageCommand;
            this.PageMessage = PageMessage;
        }
    }
}
