using System.IO;
using Wb_star_bot.Telegram_Bot;
using Wb_star_bot.Clients;
using Wb_star_bot.Wb_handler;
using Wb_star_bot;
using System.Net;

string token = "5719447713:AAF-7w3jQQnvs2v9ZjzJ-5nEL61fzYD0n8M";

Bot bot = new Bot(token, new Dictionary<string, BotPage[]>
{

    {"/my",new BotPage[]{
        new BotPage(0, Bot.GetClientAccountInfo),
    }},

    {"/income", Pages.incomes},

    {"/stocks", new BotPage[]{
        new BotPage(0, WbBaseManager.GetStocksData),
    }},
    {"/sales", new BotPage[]{
        new BotPage(0, WbBaseManager.GetSalesData),
    }},
    {"/orders", new BotPage[]{
        new BotPage(0, WbBaseManager.GetOrdersData),
    }},
});

while (true)
{
    string? cmd = Console.ReadLine() ?? null;

    if (cmd == null) continue;

    switch (cmd)
    {
        case "end": return;

    }
}