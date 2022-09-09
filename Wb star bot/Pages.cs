using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wb_star_bot.Telegram_Bot;
using Wb_star_bot.Wb_handler;

namespace Wb_star_bot
{
    internal static class Pages
    {
        public static BotPage[] incomes = new BotPage[]{
        new BotPage(0, WbBaseManager.GetIncomeData,
        new (string text, int page)[]
        {
            ("Сегодня", 1),
            ("Вчера", 2),
        },
        new (string text, int page)[]
        {
            ("Текущий месяц", 3),
            ("Прошлый месяц", 4),
        }),

        new BotPage(1, WbBaseManager.IncomeToday,
        new (string text, int page)[]
        {
            ("Сегодня 🔵", -1),
            ("Вчера", 2),
        },
        new (string text, int page)[]
        {
            ("Текущий месяц", 3),
            ("Прошлый месяц", 4),
        },
        new (string text, int page)[]
        {
            ("Назад", 0),
        }),

        new BotPage(2, WbBaseManager.IncomeLastToday,
        new (string text, int page)[]
        {
            ("Сегодня", 1),
            ("Вчера 🔵", -1),
        },
        new (string text, int page)[]
        {
            ("Текущий месяц", 3),
            ("Прошлый месяц", 4),
        },
        new (string text, int page)[]
        {
            ("Назад", 0),
        }),

        new BotPage(3, WbBaseManager.IncomeMounth,
        new (string text, int page)[]
        {
            ("Сегодня", 1),
            ("Вчера", 2),
        },
        new (string text, int page)[]
        {
            ("Текущий месяц 🔵", -1),
            ("Прошлый месяц", 4),
        },
        new (string text, int page)[]
        {
            ("Назад", 0),
        }),

        new BotPage(4, WbBaseManager.IncomeLastMounth,
        new (string text, int page)[]
        {
            ("Сегодня", 1),
            ("Вчера", 2),
        },
        new (string text, int page)[]
        {
            ("Текущий месяц", 3),
            ("Прошлый месяц 🔵", -1),
        },
        new (string text, int page)[]
        {
            ("Назад", 0),
        })};

    }
}
