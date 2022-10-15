﻿using System.IO;
using Wb_star_bot.Telegram_Bot;
using Wb_star_bot.Clients;
using Wb_star_bot.Wb_handler;
using Wb_star_bot;
using System.Net;

//68507544 медицинский костюм 4 60

string token = "5719447713:AAF-7w3jQQnvs2v9ZjzJ-5nEL61fzYD0n8M";

Bot bot = new Bot(token, new Dictionary<string, BotPage[]>
{
    {"/pay", new BotPage[]{
        new BotPage(0, BotPay.AccountPay),
    }},
    {"/tarif", new BotPage[]{
        new BotPage(0, BotPay.TarifSelectTable),
    }},
    {"/my",new BotPage[]{
        new BotPage(0, Bot.GetClientAccountInfo),
    }},
    {"/search", new BotPage[]{
        new BotPage(0, WbBaseManager.GetProductPosition),
    }},
    {"/ads", new BotPage[]{
        new BotPage(0, WbBaseManager.GetCategoryAds),
    }},
});

while (true)
{
    string? cmd = Console.ReadLine() ?? null;

    if (cmd == null) continue;

    switch (cmd)
    {
      //  case "test":WbBaseManager.test(bot);break;
        case "end": return;

    }
}