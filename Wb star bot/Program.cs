﻿using System.IO;
using Wb_star_bot.Telegram_Bot;
using Wb_star_bot.Clients;
using Wb_star_bot.Wb_handler;
using Wb_star_bot;
using System.Net;
    
string token = "5719447713:AAF-7w3jQQnvs2v9ZjzJ-5nEL61fzYD0n8M";

if (!Directory.Exists(WbBaseManager.output))
{
    Directory.CreateDirectory(WbBaseManager.dataDirectory);
}

Console.WriteLine($"Current bot files directory: {WbBaseManager.dataDirectory}");


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
    {"/products",new BotPage[]{
        new BotPage(0, WbBaseManager.ClientDataOrders),
    }},
    {"/label", new BotPage[]{
        new BotPage(0, WbBaseManager.ProductInfo),
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
        case "echo 0":
            bot.consoleLog[0] = !bot.consoleLog[0];
            Console.WriteLine($"Messages: {(bot.consoleLog[0] ? "on" : "off")}");
            break;
        case "echo 1":
            bot.consoleLog[1] = !bot.consoleLog[1];
            Console.WriteLine($"Querry datas: {(bot.consoleLog[1] ? "on" : "off")}");
            break;
        case "echo 2":
            WbBaseManager.showUpdMessages= !WbBaseManager.showUpdMessages;
            Console.WriteLine($"Uupdating datas: {(WbBaseManager.showUpdMessages ? "on" : "off")}");
            break;
        case "end": await bot.QuitMessage(); return;

    }
}