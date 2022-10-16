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
using Telegram.Bot.Types.Payments;
using System.Collections.Generic;
using Wb_star_bot.Wb_handler;
using Wb_star_bot.Clients;

namespace Wb_star_bot.Telegram_Bot
{
    internal delegate string CommandCallback(Bot bot, ClientData[]? client);
    internal delegate (string, InlineKeyboardMarkup?) QueryCallback(Bot bot, ClientData[]? client, object? query);
    internal delegate void MessageCallback(Bot bot, ClientData[]? client, Message? message);

    internal class Bot
    {
        public const int MaxMessageLenght = 2048; // tg limit is 4096
        public const int MaxPageLenght = 2048; // tg limit is 4096

        public Dictionary<string, ClientData> clientDatas = new Dictionary<string, ClientData>();
        public Dictionary<long, Client> clientBook = new Dictionary<long, Client>();

        public Dictionary<string, BotPage[]> commands;

        public ITelegramBotClient botClient;

        public static Dictionary<answer, string> answerList = new Dictionary<answer, string>()
        {
            {answer.start, "😭 Вы еще не прошли аунтификацию в Wb star bot.\n⭐️ *Пожалуйста, используйте команду /start*."},
            {answer.enter_api, "*Введите Api64*\n\n1️⃣ Зайдите в аккаунт Wilberries Partners.\n\n2️⃣ Перейдите в Профиль -> Настройки -> Доступ к API, или вольпользуйтесь [ссылкой](https://seller.wildberries.ru/login/ru?redirect_url=/supplier-settings/access-to-api).\n\n3️⃣ Отправьте ваш Api ключ для ведения статистики x64, без пробелов и прочих символов."},
            {answer.error_api, "❌ Api64 ключ введен некорректно.\n\nПроверьте правильность ключа, возможно вы ввели не тот ключ или указали его не полностью.\n\nОтправьте правильный ключ заново в этом сообщении:"},
            {answer.acc_exists, "✅ Вы уже используете WB Star Bot."},
            {answer.data_bad_request, "❌ Не удалось привязать данный Api64 ключ.\n\n🔁 Проверьте актуальность введённого ключа, согласно инструкции и повторите попытку."},
            {answer.data_too_many_requests, "❌ Сервисы Wildberries в данный момент не доступны.\n\n👨‍💻 Техническая поддержка WB Star Bot уже работает над данной проблемой.\n\n🔁 Повторите попытку позже или обратитесь в тех поддержку."},
            {answer.data_successfuly, "✅ *Апи успешно активирован!*\n\nТеперь бот будет присылать статистику по Вашим заказам. Редактирование частоты и формата сообщений доступно в *Личном кабинете* в меню действий.\n\n⚠️ Если вы используете систему *FBS* или хотите расширить функционал бота, сгенерируйте уникальный Api ключ в WB Partners по [ссылке](https://seller.wildberries.ru/supplier-settings/access-to-new-api). И привяжите его к аккаунту в *Личном кабинете* или нажав на кнопку ниже."},
            {answer.data_failed, "❌ Не удалось получить данные по введенному Api.\n\n🔁 Повторите попытку через пару минут или обратитесь в нашу техническую поддержку."},
            {answer.data_already_has_reciver, "⚠️ Данный Api уже используется к на данном устройстве." },
            {answer.pay_succes, "✅ Баланс аккаунта успешно пополнен!"},

            {answer.unk_command, "❌ Неизвестная команда.\n\n⬇️ Используйте доступные команды из *списка*."},
            {answer.bot_info, "👋 Всем селлерам привет!\n🤖 Добро пожаловать на *закрытый бесплатный* бета тест-нашего бота.\n\n❇️ *Что умеет наш бот:*\n\n1️⃣ Уведомления о заказах/возвратах/покупках.\n\n2️⃣ Нахождение точной позиции артикула в поисковой системе Wildberries.\n\n3️⃣ Детальная выписка по остаткам на складах и рекомендации по отгрузке следующей партии.\n\n4️⃣ Подведение итогов ваших продаж в «закреплённых сообщениях».\n\n5️⃣ Отчеты по ИП, бренду, категории товара за нужный вам период времени.\n\n6️⃣ Возможность добавления самовыкупов, что позволит вам видеть вашу объективную прибыль.\n\n7️⃣ Отслеживание реальной стоимости рекламы по поисковой выдаче, исходя из ставки конкурентов.\n\n8️⃣ Автоматическая реклама, позволяющая удержать позицию вашего товара, без вашего участия, за желаемую цену.\n\n9️⃣ Прогнозы и расчеты по вашим товарам, остаткам и складам." }
        };

        public static string[] smiles = new string[]
        {
            "🍒",
            "🥝",
            "🍓",
            "🍎",
            "🍏",
            "🌽",
            "🍅",
            "🍌",
            "🍉",
        };

        public Bot(string token, Dictionary<string, BotPage[]> commands)
        {
            botClient = new TelegramBotClient(token);

            this.commands = commands;
            Console.WriteLine("Запущен бот " + botClient.GetMeAsync().Result.FirstName);

            botClient.StartReceiving(
                ReciveHandler,
                ErrorHandler,
                new ReceiverOptions
                {
                    AllowedUpdates = { },
                },
                new CancellationTokenSource().Token
            );
            new Thread(new ParameterizedThreadStart(WbBaseManager.ClientDataUpdating)).Start(this);
            new Thread(new ParameterizedThreadStart(WbBaseManager.DailyMessage)).Start(this);
        }

        public async Task ReciveHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                        await QueryHandler(update.CallbackQuery);
                        break;
                    case Telegram.Bot.Types.Enums.UpdateType.Message:
                        if (update.Message?.SuccessfulPayment != null)
                        {
                            ClientData currentClient = clientDatas[update.Message.SuccessfulPayment.InvoicePayload];
                            if (!currentClient.AddBalance(update.Message.SuccessfulPayment.TotalAmount / 100))
                            {
                                await SendMessage(update.Message.Chat.Id, answerList[answer.data_successfuly]);
                            }
                            await SendMessage(update.Message.Chat.Id, answerList[answer.pay_succes]);

                            if(currentClient.tarif == ClientData.subscibeType.none)
                            {
                                (string, InlineKeyboardMarkup?) tarif = BotPay.TarifSelectTable(this, new ClientData[] {currentClient}, update.Message.Chat.Id);

                                Task<Message> snd = new Task<Message>(() => SendMessage(update.Message.Chat.Id, tarif.Item1, tarif.Item2).Result);
                                snd.Start();
                                clientBook[update.Message.Chat.Id].currentPage = snd.Result.MessageId;
                            }
                        }
                        else
                        {
                            await MessageHandler(update.Message);
                        }
                        break;
                    case Telegram.Bot.Types.Enums.UpdateType.PreCheckoutQuery:
                        await PreCheckoutHandler(update.PreCheckoutQuery);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public async Task PreCheckoutHandler(PreCheckoutQuery query)
        {
            
            await botClient.AnswerPreCheckoutQueryAsync(query.Id);
        }

        public async Task<Message> SendMessage(long senderId, string text, IReplyMarkup? markup = null)
        {
            if (text.Length > MaxMessageLenght)
                text = text.Remove(MaxMessageLenght);
            return await botClient.SendTextMessageAsync(senderId, text, replyMarkup: markup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
        }

        public async Task SendMessage(long senderId, string text, FileStream stream, IReplyMarkup? markup = null)
        {
            if (text.Length > MaxMessageLenght)
                text = text.Remove(MaxMessageLenght);

            await botClient.SendPhotoAsync(senderId, new InputOnlineFile(stream, "photo"), text, replyMarkup: markup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
        }

        public async Task QueryHandler(CallbackQuery query)
        {
            long senderId = query.Message.Chat.Id;


            if (!clientBook.ContainsKey(senderId))
                return;

            ClientData[]? caller = GetClient(senderId);

            string[] cmds = query.Data.Split(' ');
            string cmd = cmds[0];


            if(cmd == "~" || cmds[cmds.Length-1] == "~")
            {
                await botClient.DeleteMessageAsync(query.Message.Chat, query.Message.MessageId);
                if (cmd == "~")
                return;
            }


            if (commands[cmd][0].queryCallback == null)
            {
                int page = int.Parse(cmds[1]);
                if (page == -1)
                    return;
                string text = commands[cmd][page].callback.Invoke(this, caller);

                if (text.Length > MaxMessageLenght)
                    text = text.Remove(MaxMessageLenght);

                InlineKeyboardMarkup markup = GetCommandMarkup(cmd, page);

                await botClient.EditMessageTextAsync(query.Message.Chat, query.Message.MessageId, text, replyMarkup: markup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
            }
            else
            {
                try
                {
                    Client curr = clientBook[senderId];

                    if (curr.currentPage == null || curr.currentPage == query.Message.MessageId)
                    {
                        (string, InlineKeyboardMarkup?)? answ = curr.queryCallback?.Invoke(this, caller, query);

                        if (answ != null && answ.Value.Item1 != null)
                        {
                            curr.currentPage = query.Message.MessageId;
                            await botClient.EditMessageTextAsync(query.Message.Chat, query.Message.MessageId, answ.Value.Item1, replyMarkup: answ.Value.Item2, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

                            Console.WriteLine(query.Data);
                        }
                    }
                    else
                    {
                        dlt();
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    dlt();
                }

                async void dlt()
                {
                    await botClient.DeleteMessageAsync(query.Message.Chat, query.Message.MessageId);

                    (string, InlineKeyboardMarkup?) ans = commands[cmd][0].queryCallback?.Invoke(this, caller, senderId) ?? ("Не известаня команда", null);
                    Task<Message> snd = new Task<Message>(() => SendMessage(senderId, ans.Item1, ans.Item2).Result);
                    snd.Start();
                    clientBook[senderId].currentPage = snd.Result.MessageId;
                }
            }
        }

        public async Task MessageHandler(Message update)
        {
            
            string message = update.Text ?? "";
            long senderId = update.Chat.Id;

            if (message == "/start")
            {
                if (!clientBook.ContainsKey(senderId))
                {
                    clientBook.Add(senderId, new Client());
                    clientBook[senderId].messageCallback = MessageCallback;
                    await SendMessage(senderId, answerList[answer.enter_api]);
                }
                else
                {
                    await SendMessage(senderId, answerList[answer.acc_exists]);
                }
            }else if(message == "/info")
            {
                await SendMessage(senderId, answerList[answer.bot_info]);
            }
            else if (!clientBook.ContainsKey(senderId))
            {
                await SendMessage(senderId, answerList[answer.start]);
            }
            else
            {
                if (commands.ContainsKey(message))
                {
                    ClientData[]? caller = GetClient(senderId);

                    if (caller == null || caller.Length == 0)
                    {
                        clientBook[senderId].messageCallback = MessageCallback;
                        await SendMessage(senderId, answerList[answer.enter_api]);
                        return;
                    }

                    if (commands[message][0].queryCallback is not null)
                    {
                        (string, InlineKeyboardMarkup?) ans = commands[message][0].queryCallback?.Invoke(this, caller, senderId) ?? ("Не известаня команда", null);
                        Task<Message> snd = new Task<Message>(()=> SendMessage(senderId, ans.Item1, ans.Item2).Result);
                        snd.Start();
                        clientBook[senderId].currentPage = snd.Result.MessageId;
                    }
                    else
                    {
                        await SendMessage(senderId, commands[message][0].callback.Invoke(this, caller), GetCommandMarkup(message, 0));
                    }

                    await botClient.DeleteMessageAsync(update.Chat, update.MessageId);
                }
                else if (clientBook[senderId].messageCallback != null)
                {
                    clientBook[senderId].messageCallback.Invoke(this, GetClient(senderId), update);
                }
                else
                {
                    await SendMessage(senderId, answerList[answer.unk_command]);
                }
            }

            Console.WriteLine($"{senderId}: {message}");
        }

        public async void SendInvoce(long chatId, string apikey, string name, int summ)
        {
            await botClient.SendInvoiceAsync(chatId, "Пополнение аккаунта", $"Оплата личного счета {name}.", apikey, "390540012:LIVE:26970", "RUB", new LabeledPrice[] { new LabeledPrice("Оплатить", summ * 100) });
        }
        public async void MessageCallback(Bot bot, ClientData[]? client, Message? mes)
        {
            string message = mes?.Text ?? "";
            long senderId = mes?.Chat.Id ?? 0;

            if (message.Length != 48 || message.Contains(' '))
            {
                await SendMessage(senderId, answerList[answer.error_api]);
            }
            else
            {
                try
                {
                    await AddNewClient(message, senderId);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await SendMessage(senderId, answerList[answer.error_api]);
                    return;
                }
            }
        }

        public async Task AddNewClient(string message, long senderId)
        {

            if (clientDatas.ContainsKey(message))
            {
                if (clientDatas[message].recivers.Contains(senderId))
                {
                    await SendMessage(senderId, answerList[answer.data_already_has_reciver]);
                    return;
                }

                clientDatas[message].recivers.Add(senderId);
                await SendMessage(senderId, answerList[answer.data_successfuly]);

                if (!clientDatas[message].active)
                {
                    (string, InlineKeyboardMarkup) its = BotPay.AccountPay(this, new ClientData[] { clientDatas[message] }, senderId);

                    Task<Message> snd = new Task<Message>(() => SendMessage(senderId, its.Item1, its.Item2).Result);
                    snd.Start();
                    clientBook[senderId].currentPage = snd.Result.MessageId;
                }
            }
            else
            {
                ClientData newClient = new ClientData(message, senderId);
                int c = clientBook[senderId].clientDatas.Count;

                newClient.Smile = c>= smiles.Length ? "" : smiles[c];

                try
                {
                    await WbBaseManager.update(message, null, newClient.stocksData, "stocks");
                    await WbBaseManager.update(message, null, newClient.ordersData, "orders", "&flag=0", GetAccInfo);


                    void GetAccInfo()
                    {
                        if (newClient.ordersData.orders.Count > 0)
                        {
                            newClient.Name = WbBaseManager.getAccountInfo(newClient.ordersData.orders.ElementAt(0).Value.nmId);
                        }
                    }
                }
                catch (WbException ex)
                {
                    if (ex.exceptionType == WbException.ExceptionType.data_too_many_request)
                    {
                        await SendMessage(senderId, answerList[answer.data_too_many_requests]);
                    }
                    else if (ex.exceptionType == WbException.ExceptionType.data_bad_request)
                    {
                        await SendMessage(senderId, answerList[answer.data_bad_request]);
                    }
                    Console.WriteLine(ex.ToString());
                    return;
                }

                clientDatas.Add(message, newClient);
                (string, InlineKeyboardMarkup) its = BotPay.AccountPay(this, new ClientData[] { newClient }, senderId);
                Task<Message> snd = new Task<Message>(() => SendMessage(senderId, its.Item1, its.Item2).Result);
                snd.Start();
                clientBook[senderId].currentPage = snd.Result.MessageId;
            }

            clientBook[senderId].clientDatas.Add(message);

            clientBook[senderId].messageCallback = null;
        }

        public async Task ErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception);
        }

        public InlineKeyboardMarkup? GetCommandMarkup(string command, int page)
        {
            BotPage botPage = commands[command][page];

            if (botPage.links == null) return null;

            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[botPage.links.Length][];

            for (int i = 0; i < botPage.links.Length; i++)
            {
                buttons[i] = new InlineKeyboardButton[botPage.links[i].Length];

                for (int j = 0; j < botPage.links[i].Length; j++)
                {
                    (string text, int page) cur = botPage.links[i][j];
                    buttons[i][j] = new InlineKeyboardButton(cur.text)
                    {
                        CallbackData = $"{command} {cur.page}",
                    };
                }
            }
            return buttons;
        }

        public ClientData[]? GetClient(long senderId)
        {
            if (!clientBook.ContainsKey(senderId)) return null;

            ClientData[] datas = new ClientData[clientBook[senderId].clientDatas.Count];

            for (int i = 0; i < datas.Length; i++)
            {
                datas[i] = clientDatas[clientBook[senderId].clientDatas[i]];
            }
            return datas;
        }

        public static (string, InlineKeyboardMarkup?) GetClientAccountInfo(Bot bot, ClientData[]? client, object? query)
        {
            if (client == null || query == null) throw new Exception("Client is null");

            string content = "Ваши ИП:";

            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[client.Length + 1][];

            for (int i = 0; i < client.Length; i++)
            {
                buttons[i] = new InlineKeyboardButton[1]
                {
                    new InlineKeyboardButton($"{client[i].Smile} {client[i].Name}")
                    {
                        CallbackData = $"/my {i}",
                    },
                };
            }

            buttons[client.Length] = new InlineKeyboardButton[1]
            {
                    new InlineKeyboardButton("+ Добавить")
                    {
                        CallbackData = "/my +",
                    },
            };

            long senderId = (long)query;

            bot.clientBook[senderId].queryCallback = EditAccount;

            return (content, buttons);
        }

        public static async void AddNewAccount(Bot bot, ClientData[]? client, Message? message)
        {
            string txt = message?.Text ?? "";
            long senderId = message.Chat.Id;

            if (txt.Length != 48 || txt.Contains(' '))
            {
                await bot.SendMessage(senderId, answerList[answer.error_api]);
            }
            else
            {
                try
                {
                    await bot.AddNewClient(txt, senderId);
                }
                catch (Exception e)
                {
                    await bot.SendMessage(senderId, answerList[answer.error_api]);
                    return;
                }
            }
            bot.clientBook[senderId].messageCallback = null;
        }

        public static (string, InlineKeyboardMarkup?) EditAccount(Bot bot, ClientData[]? client, object? arg)
        {
            CallbackQuery query = arg as CallbackQuery;

            long senderId = query.Message.Chat.Id;
            string[] args = query.Data.Split(" ");

            if (args[1] == "+")
            {
                bot.clientBook[senderId].messageCallback = AddNewAccount;
                return (answerList[answer.enter_api], null);
            }
            else if (args[1] == "<")
            {
                return GetClientAccountInfo(bot, client, senderId);
            }else if (args.Length > 2)
            {

                if (args[2] == "pay")
                {
                    return ("⬇️ Выберите сумму оплаты:\n\n🔰 Ваш бонус к пополнению: 0%", BotPay.getPayButtons("my", $" {args[1]} ~"));
                }else if (args[2] == "p")
                {
                    bot.clientBook[senderId].messageCallback = (a, b, c) => BotPay.ActivePromocode(a, new ClientData[] { currentClient() }, c);
                    return ($"🎟️ Введите промокод:", new InlineKeyboardButton("Назад") { CallbackData = $"/my back {args[1]}" });
                }
                else if (args[1] == "any")
                {
                    bot.clientBook[senderId].messageCallback = (a, b, c) => BotPay.SelectPrice(a, new ClientData[] { client[int.Parse(args[2])] }, c);
                    return ($"⬇️ Введите желаюмую сумму пополнения.\n\n🔰 Ваш бонус к пополнению: 0%\n\n⚠️ Минимальная сумма пополнения: {BotPay.minPaySumm} руб.",  new InlineKeyboardButton("Назад") { CallbackData = $"/my back {args[2]}" });
                }else if (args[2] == "tarif")
                {
                    ClientData currentData = currentClient();
                    bot.clientBook[senderId].messageCallback = null;
                    return ($"{(currentData.tarif == ClientData.subscibeType.none ? "⭕️ Текущий тариф: Не выбран" : "✨ Текущий тариф: " + (currentData.tarif == ClientData.subscibeType.simple ? "Стандарт" : "Премиум"))}\n\n⚠️ Внимание! При выборе более дорогого тарифа будет списана разница между текущим и выбранным тарифом. Однако, при смене тарифа на более дешевый разница стоимости *возмещена не будет!*\n\n💰 Текущий баланс: {currentData.balance} руб.",
                        new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("🥈 Тариф стандарт") { CallbackData = $"/my {args[1]} s1" } },
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("🥇 Тариф премиум") { CallbackData = $"/my {args[1]} s2" } },
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("Назад") { CallbackData = $"/my back {args[1]}" } },
                    }));
                }
                else if (args[2] == "s1")
                {
                    ClientData currentData = currentClient();

                    if (currentData.tarif == ClientData.subscibeType.simple)
                    {
                        return ($"❇️ Текущий тариф \n\n{BotPay.standartTarifSummary}\n\n💰 *Стоимость тарифа: {ClientData.standartTarifCost}₽ в мес.*", new InlineKeyboardButton("Назад") { CallbackData = $"/my {args[1]} tarif" });
                    }
                    return ($"{BotPay.standartTarifSummary}\n\n💰 *Стоимость тарифа: {ClientData.standartTarifCost}₽ в мес.*",
                        new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("Выбрать") { CallbackData = $"/my {args[1]} {(currentData.tarif == ClientData.subscibeType.premium ? "ss3" : "ss1")}" } },
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("Назад") { CallbackData = $"/my {args[1]} tarif" }},
                    }));
                }
                else if (args[2] == "s2")
                {
                    ClientData currentData = currentClient();

                    if (currentData.tarif == ClientData.subscibeType.premium)
                    {
                        return ($"❇️ Текущий тариф\n\n{BotPay.premiumTarifSummary}\n\n💰 *Стоимость тарифа: {ClientData.premiumTarifCost}₽ в мес.*", new InlineKeyboardButton("Назад") { CallbackData = $"/my {args[1]} tarif" });
                    }
                    return ($"{BotPay.premiumTarifSummary}\n\n💰 *Стоимость тарифа: {ClientData.premiumTarifCost}₽ в мес.*",
                        new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("Выбрать") { CallbackData = $"/my {args[1]} ss2" } },
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("Назад") { CallbackData = $"/my {args[1]} tarif" }},
                    }));
                }
                else if (args[2] == "ss1")
                {
                    ClientData currentData = currentClient();
                    if (currentData.SelectTarif(ClientData.subscibeType.simple))
                    {
                        return ($"✅ Тариф \"Стандарт\" успешно акутивирован на аккаунте \"{currentData.Name}\"", null);
                    }
                    return (BotPay.balanceLow, new InlineKeyboardButton("Назад") { CallbackData = $"/my {args[1]} tarif" });                
                }
                else if (args[2] == "ss2")
                {
                    ClientData currentData = currentClient();
                    if (currentData.SelectTarif(ClientData.subscibeType.premium))
                    {
                        return ($"✅ Тариф \"Премиум\" успешно акутивирован на аккаунте \"{currentData.Name}\"", null);
                    }
                    return (BotPay.balanceLow, new InlineKeyboardButton("Назад") { CallbackData = $"/my {args[1]} tarif" });
                }
                else if (args[2] == "ss3")
                {
                    ClientData currentData = currentClient();

                    return ($"⚠️ Вы уверены, что хотите изменить свой тариф на более дешевый?\n\nℹ️ Некоторые функции могут стать не доступны, а разница стоимости между текущим и выбранным тарифом возмещена *не будет!*", new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("Выбрать") { CallbackData = $"/my {args[1]} ss1" } },
                        new InlineKeyboardButton[]{ new InlineKeyboardButton("Назад") { CallbackData = $"/my {args[1]} tarif" }},
                    }));
                }
                else if (args[1] != "back")
                {
                    BotPay.SelectPrice(bot, new ClientData[] { client[int.Parse(args[2])] }, senderId, int.Parse(args[1]));
                    return (null, null);
                }
                else
                {
                    args[1] = args[2];
                    bot.clientBook[senderId].messageCallback = null;
                    return editAcc();
                }
            }
            else
            {
                return editAcc();
            }
            return ("Редактирование профиля еще не доступно", new InlineKeyboardMarkup(new InlineKeyboardButton[] { new InlineKeyboardButton("Назад") { CallbackData = "/my <" } }));


            (string, InlineKeyboardMarkup ?) editAcc()
            {
                int acId = int.Parse(args[1]);
                ClientData currentData = currentClient();
                string AccountSummary = $"{currentData.Smile} {currentData.Name}\n\n";
                AccountSummary += $"{(currentData.tarif == ClientData.subscibeType.none ? "⭕️ Тариф: Не выбран" : "✨ Тариф: " + (currentData.tarif == ClientData.subscibeType.simple ? "Стандарт" : "Премиум"))}\n";
                AccountSummary += $"🔰 Реферальный бонус: {currentData.promocode}\n";
                AccountSummary += $"💰 Текущий баланс: {currentData.balance} руб.";

                return (AccountSummary, new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new InlineKeyboardButton[]{ new InlineKeyboardButton("💰 Пополнить баланс") { CallbackData = $"/my {acId} pay" } },
                new InlineKeyboardButton[]{ new InlineKeyboardButton("⭐️ Выбрать тариф") { CallbackData = $"/my {acId} tarif" } },
                new InlineKeyboardButton[]{ new InlineKeyboardButton("🎟️ Использовать промокод") { CallbackData = $"/my {acId} p" } },
                new InlineKeyboardButton[]{ new InlineKeyboardButton("Назад") { CallbackData = "/my <" } },
                
                }));
            }

            ClientData currentClient() { return client[int.Parse(args[1])]; }
        }
    }

    public enum answer : byte
    {
        start = 0,
        enter_api = 1,
        error_api = 2,
        acc_exists = 3,

        data_bad_request = 4,
        data_too_many_requests = 5,
        data_successfuly = 6,
        data_failed = 7,
        data_already_has_reciver = 8,

        pay_succes = 32,

        bot_info = 127,
        unk_command = 128,
    }
}
