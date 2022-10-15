using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;
using Wb_star_bot.Wb_handler;
using Wb_star_bot.Clients;

namespace Wb_star_bot.Telegram_Bot
{
    internal class BotPay
    {
        public const int minPaySumm = 60;

        public const string standartTarifSummary = "🥈 *Тариф \"Стандарт\"*\n\nℹ️ Функции:\n\n1️⃣ Уведомления о новых заказах.\n\n2️⃣ Ежедневная статистика.\n\n3️⃣ Поиск позиции товара по категории на первых 50 страницах.\n\n4️⃣ Отслеживание реальной стоимости рекламной позиции.\n\n5️⃣ Онлайн техническая поддержка.\n\n📪 Стоимость уведомлений о заказах:\n\nДо 10 000: *3₽ за 100 уведомлений*.\n\nОт 10 000: *0.5₽ за 100 уведомлений*.\n\n⚠️ Счетчик уведомлений о заказах обнуляется в каждый месяц с даты приобритения подписки.";
        public const string premiumTarifSummary = "🥇 *Тариф \"Премиум\"*\n\nℹ️Функции:\n\n1️⃣ Уведомления о новых заказах.\n\n2️⃣ Ежедневная *полная* статистика с таблицами _(в разработке)_.\n\n3️⃣ Поиск позиции товара по категории на первых 50 страницах по *разным регионам*.\n\n4️⃣ Отслеживание реальной стоимости рекламной позиции.\n\n5️⃣ Автоматическая реклама, для удержания вашего товара в нужной позиции поиска _(в разработке)_.\n\n6️⃣ Доступ к полной статистике и отчетам _(в разработке)_.\n\n7️⃣ Онлайн техническая поддержка.\n\n📪 Стоимость уведомлений о заказах:\n\nДо 10 000: *3₽ за 100 уведомлений*.\n\nОт 10 000: *0.5₽ за 100 уведомлений*.\n\n⚠️ Счетчик уведомлений о заказах обнуляется в каждый месяц с даты приобритения подписки.";
        public const string balanceLow = "❌ На вашем балансе недостаточно средств";


        public static InlineKeyboardButton[][] getPayButtons(string page, string apiKey, string? endArg) => new InlineKeyboardButton[][] {
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("200 руб.") {CallbackData =  $"/{page} 200 {apiKey}{endArg}", } },
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("500 руб.") {CallbackData =  $"/{page} 500 {apiKey}{endArg}", } },
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("1000 руб.") {CallbackData =  $"/{page} 1000 {apiKey}{endArg}", } },
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("Другая сумма") {CallbackData =  $"/{page} any {apiKey}", } },
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("Назад") {CallbackData = $"/{page} back {apiKey}", } }, };


        public static (string, InlineKeyboardMarkup?) AccountPay(Bot bot, ClientData[]? client, object? query)
        {
            ClientData handleData = client[0];

            if (handleData.balance == 0)
            {
                bot.clientBook[(long)query].queryCallback = (a,b,c) => AccountPaySelect(a,new ClientData[] { handleData },c);
                return ($"❌ Данный аккаунт на текущий момент не активен.\n\nℹ️ Чтобы активировать аккаунт необходимо пополнить баланс и выбрать *подходящий тариф* или воспользоваться *промокодом*. Изменение тарифа всегда доступно в личном кабинете.\n\n💰 Текущий баланс: 0 руб.", payKeyboard(handleData.apiKey) ?? "");
            }

            return TarifSelectTable(bot, client, query);
        }

        public static (string, InlineKeyboardMarkup?) TarifSelectTable(Bot bot, ClientData[]? client, object? query)
        {
            ClientData handleData = client[0];

            if (handleData == null)
            {
                return ($"⚠️ Выберите нужный аккаунт в личном кабинете.", null);
            }

            long senderId = (long)query;

            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[][]
                {
                new InlineKeyboardButton[]{ new InlineKeyboardButton("🥈 Тариф стандарт") {CallbackData = $"/tarif 1 {handleData.apiKey}", } },
                new InlineKeyboardButton[]{ new InlineKeyboardButton("🥇 Тариф премиум") {CallbackData =  $"/tarif 2 {handleData.apiKey}", } },
                new InlineKeyboardButton[]{ new InlineKeyboardButton("💵 Пополнить баланс") { CallbackData = $"/pay pay {handleData.apiKey}", }},
                new InlineKeyboardButton[]{ new InlineKeyboardButton("🎟️ Использовать промокод") { CallbackData = $"/pay p {handleData.apiKey}", }},
                };

            bot.clientBook[(long)query].queryCallback =(a,b,c)=> TarifInfo(a,client, c);
            string currentTarif = "⚠️ Вы еще не выбрали тариф для вашего аккаунта.";
            switch (handleData.tarif)
            {
                case ClientData.subscibeType.simple:
                    currentTarif = "Текущий тариф: стандарт";
                    break;
                case ClientData.subscibeType.premium:
                    currentTarif = "Текущий тариф: премиум";
                    break;
            }

            return ($"{currentTarif}\n\n⬇️ Нажмите на интересующий вас тариф чтобы ознакомится с условиями и функционалом. Сменить тариф можно позже, в *любое время*!\n\n💰 Текущий баланс: {handleData.balance} руб.", buttons);

        }

        public static InlineKeyboardMarkup payKeyboard(string querry = null)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[][]
            {
                new InlineKeyboardButton[]{ new InlineKeyboardButton("💵 Пополнить баланс"){CallbackData = $"/pay pay {querry}",} },
                new InlineKeyboardButton[]{ new InlineKeyboardButton("🎟️ Использовать промокод") { CallbackData = $"/pay p {querry}", }},
                new InlineKeyboardButton[]{ new InlineKeyboardButton("⭐️ Тарифы") { CallbackData = $"/tarif {querry}", }},
            };
            return buttons;
        }

        public static (string, InlineKeyboardMarkup?) TarifInfo(Bot bot, ClientData[]? client, object? arg)
        {
            CallbackQuery query = arg as CallbackQuery;
            long senderId = query.Message.Chat.Id;
            string[] args = query.Data.Split(" ");
            InlineKeyboardButton[][] buttons = null;
            string summary = "";

            switch (args[1])
            {
                case "1":
                    buttons = new InlineKeyboardButton[][]
                    {
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("Выбрать") {CallbackData =  $"/tarif s1 {args[2]}", } },
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("Назад") {CallbackData = $"/tarif back {args[2]}", } },
                    };
                    bot.clientBook[senderId].queryCallback = (a,b,c)=> TarifSelect(a,client,c);
                    summary = $"{standartTarifSummary}\n\n💰 *Стоимость тарифа: {ClientData.standartTarifCost}₽ в месяц*";
                    break;
                case "2":
                    buttons = new InlineKeyboardButton[][]
                    {
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("Выбрать") {CallbackData =  $"/tarif s2 {args[2]}", } },
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("Назад") {CallbackData = $"/tarif back {args[2]}", } },
                    };
                    bot.clientBook[senderId].queryCallback = (a, b, c) => TarifSelect(a, client, c);
                    summary = $"{premiumTarifSummary}\n\n💰 *Стоимость тарифа: {ClientData.premiumTarifCost}₽ в месяц*";
                    break;
                case "pay":
                    return ("⬇️ Выберите сумму оплаты:\n\n🔰 Ваш бонус к пополнению: 0%", getPayButtons("tarif", args[2], " ~"));
                case "p":
                    bot.clientBook[senderId].messageCallback = (a, b, c) => ActivePromocode(a, new ClientData[] { bot.clientDatas[args[2]] }, c);
                    return ("🎟️ Введите промокод:", new InlineKeyboardButton("Назад") { CallbackData = $"/tarif back {args[2]}", });
                case "back":
                    bot.clientBook[senderId].messageCallback = null;
                    return TarifSelectTable(bot, new ClientData[] { bot.clientDatas[args[2]] }, senderId);
                case "any":
                    buttons = new InlineKeyboardButton[][]
                    {
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("Назад") {CallbackData = $"/tarif back {args[2]}", } },
                    };
                    bot.clientBook[senderId].messageCallback = (a, b, c) => SelectPrice(a, new ClientData[] { bot.clientDatas[args[2]] }, c);
                    return ($"⬇️ Введите желаюмую сумму пополнения.\n\n🔰 Ваш бонус к пополнению: 0%\n\n⚠️ Минимальная сумма пополнения: {minPaySumm} руб.", buttons);
                default:
                    SelectPrice(bot, new ClientData[] { bot.clientDatas[args[2]] }, senderId, int.Parse(args[1]));
                    break;
            }
            return (summary, buttons);
        }

        public static (string, InlineKeyboardMarkup?) TarifSelect(Bot bot, ClientData[]? client, object? arg)
        {
            CallbackQuery query = arg as CallbackQuery;
            long senderId = query.Message.Chat.Id;
            string[] args = query.Data.Split(" ");
            ClientData handleClient = bot.clientDatas[args[2]];

            switch (args[1])
            {
                case "s1":
                    if (handleClient.tarif != ClientData.subscibeType.simple)
                    {
                        if (handleClient.SelectTarif(ClientData.subscibeType.simple))
                        {
                            bot.clientBook[senderId].queryCallback = null;
                            return ($"✅ Тариф \"Стандарт\" успешно акутивирован на аккаунте \"{handleClient.Name}\"", null);
                        }
                    }
                    else
                    {
                        return ($"⚠️ Тариф \"Стандарт\" уже акутивирован на аккаунте \"{handleClient.Name}\"", new InlineKeyboardButton("Назад") { CallbackData = $"/tarif back {args[2]}" });
                    }
                    break;
                case "s2":
                    if (handleClient.tarif != ClientData.subscibeType.premium)
                    {
                        if (handleClient.SelectTarif(ClientData.subscibeType.premium))
                        {
                            bot.clientBook[senderId].queryCallback = null;
                            return ($"✅ Тариф \"Премиум\" успешно акутивирован на аккаунте \"{handleClient.Name}\"", null);
                        }
                    }
                    else
                    {
                        return ($"⚠️ Тариф \"Стандарт\" уже акутивирован на аккаунте \"{handleClient.Name}\"", new InlineKeyboardButton("Назад") { CallbackData = $"/tarif back {args[2]}" });
                    }
                    break;
                case "back":
                    return TarifSelectTable(bot, new ClientData[] { handleClient }, senderId);
            }
            return (balanceLow, new InlineKeyboardButton("Назад") { CallbackData = $"/tarif back {args[2]}" });
        }

        public static (string, InlineKeyboardMarkup?) AccountPaySelect(Bot bot, ClientData[]? client, object? query)
        {
            CallbackQuery callbackQuery = (CallbackQuery)query;
            long senderId = callbackQuery.Message.Chat.Id;
            string[] args = callbackQuery.Data.Split(" ");

            if (args[0] == "/tarif")
            {
                return TarifSelectTable(bot, client, callbackQuery.Message.Chat.Id);

            }
            Console.WriteLine(callbackQuery.Data);
            InlineKeyboardButton[][] buttons;
            switch (args[1])
            {
                case "pay":
                    return ("⬇️ Выберите сумму оплаты:\n\n🔰 Ваш бонус к пополнению: 0%", getPayButtons("pay", args[2], " ~"));
                case "p":
                    bot.clientBook[senderId].messageCallback = (a, b, c) => ActivePromocode(a, new ClientData[] { bot.clientDatas[args[2]] }, c);
                    return ("🎟️ Введите промокод:", new InlineKeyboardButton("Назад") { CallbackData = $"/pay back {args[2]}", });
                case "back":
                    bot.clientBook[senderId].messageCallback = null;
                    return AccountPay(bot, new ClientData[] { bot.clientDatas[args[2]] }, callbackQuery.Message.Chat.Id);
                case "any":
                    buttons = new InlineKeyboardButton[][]
                    {
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("Назад") {CallbackData = $"/pay back {args[2]}", } },
                    };
                    bot.clientBook[senderId].messageCallback = (a, b, c) => SelectPrice(a, new ClientData[] { bot.clientDatas[args[2]] }, c);
                    return ($"⬇️ Введите желаюмую сумму пополнения.\n\n🔰 Ваш бонус к пополнению: 0%\n\n⚠️ Минимальная сумма пополнения: {minPaySumm} руб.", buttons);
                default:
                    SelectPrice(bot, new ClientData[] { bot.clientDatas[args[2]] }, senderId, int.Parse(args[1]));
                    return ("Пополнение баланса", null);
            }

            throw new Exception("Unk command!");
        }
        public static async void SelectPrice(Bot bot, ClientData[]? client, Message? message)
        {
            string? mes = message.Text;
            long senderId = message.Chat.Id;

            int price = 0;

            if (int.TryParse(mes, out price))
            {
                if (price < minPaySumm)
                {
                    await bot.SendMessage(senderId, $"❌ Минимальная сумма пополнения: {minPaySumm} рублей.");
                }
                else
                {
                    try
                    {
                        SelectPrice(bot, client, senderId, price);
                        bot.clientBook[senderId].messageCallback = null;
                    }
                    catch (Exception e)
                    {
                        await bot.SendMessage(senderId, "❌ Указанная сумма ниже минимальной.");
                    }
                }
            }
            else
            {
                await bot.SendMessage(senderId, "❌ Не удалось прочитать сумму пополнения.\nСообщение должно содержать только число, без пробелов, букв и прочих знаков.");
            }

            await bot.botClient.DeleteMessageAsync(senderId, message.MessageId);
        }

        public static async void SelectPrice(Bot bot, ClientData[]? client, long senderId, int message)
        {
            try
            {
                bot.SendInvoce(senderId, client[0].apiKey, client[0].Name, message);
            }
            catch
            {
                await bot.SendMessage(senderId, "❌ Указанная сумма ниже минимальной.");
            }
        }

        public static async void ActivePromocode(Bot bot, ClientData[]? client, Message? message)
        {
            string? mes = message.Text;
            string callback = $"❌ Промокод \"{(mes.Length > 32 ? mes.Remove(32) + "..." : mes)}\" не активен. Повторите попытку или вернитесь к выбору оплаты.";
            await bot.botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);

            bool act = true;

            if (mes != null)
            {

                if (mes.ToLower() == "beta test")
                {
                    bot.clientBook[message.Chat.Id].messageCallback = null;

                    if (client[0].promocode == mes)
                    {
                        callback = "⚠️ Все ваши аккаунты уже ипользовали данный промокод.";

                    }
                    else
                    {
                        client[0].promocode = mes;
                        act = client[0].AddBalance(1000);
                        callback = $"✅ Промокод успешно активирован на вашем аккаунте";
                    }
                }
            }

            await bot.SendMessage(message.Chat.Id, callback, null);

            if (!act)
            {
                await bot.SendMessage(message.Chat.Id, Bot.answerList[answer.data_successfuly]);

                if (client[0].tarif == ClientData.subscibeType.none)
                {
                    (string, InlineKeyboardMarkup?) tarif = TarifSelectTable(bot, client, message.Chat.Id);

                    Task < Message > snd = new Task<Message>(() => bot.SendMessage(message.Chat.Id, tarif.Item1, tarif.Item2).Result);
                    snd.Start();
                    bot.clientBook[message.Chat.Id].currentPage = snd.Result.MessageId;
                }
            }
        }
    }
}
