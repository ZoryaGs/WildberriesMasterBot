using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wb_star_bot.Telegram_Bot
{
    internal class BotPage
    {
        public int page;

        public CommandCallback? callback;
        public (string text, int page)[][]? links;

        public QueryCallback? queryCallback;


        public BotPage(int page, CommandCallback callback, params (string text, int page)[][] links)
        {
            this.page = page;
            this.callback = callback;
            this.links = links;
        }
        public BotPage(int page, QueryCallback? queryCallback)
        {
            this.page = page;
            this.queryCallback = queryCallback;
            this.links = null;
        }   
    }
}
