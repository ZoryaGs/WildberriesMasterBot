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
            {answer.start, "Используйте команду /start."},
            {answer.enter_api, "Введите Api64. \n\n1️⃣ Зайдите в аккаунт Wilberries Partners.\n\n2️⃣ Перейдите в Профиль -> Настройки -> Доступ к API, или вольпользуйтесь [ссылкой](https://seller.wildberries.ru/login/ru?redirect_url=/supplier-settings/access-to-api).\n\n3️⃣ Отправьте ваш Api ключ для ведения статистики x64, без пробелов и прочих символов."},
            {answer.error_api, "❌ Api64 ключ введен некорректно.\n\nПроверьте правильность ключа, возможно вы ввели не тот ключ или указали его не полностью.\n\nОтправьте правильный ключ заново в этом сообщении:"},
            {answer.acc_exists, "Вы уже зарегестрированны"},
            {answer.data_bad_request, "Не удалось привязать апи. Пороверьте апи, в противном случае перевыпустите новый апи ключ"},
            {answer.data_too_many_requests, "Сервисы Wildberries сейчас не доступны. Повторите попытку через пару минут."},
            {answer.data_successfuly, "✅ *Апи успешно привязан!*\n\nТеперь бот будет присылать статистику по Вашим заказам. Редактирование частоты и формата сообщений доступно в *Личном кабинете* в меню действий.\n\n⚠️ Если вы используете систему *FBS* или хотите расширить функционал бота, сгенерируйте уникальный Api ключ в WB Partners по [ссылке](https://seller.wildberries.ru/supplier-settings/access-to-new-api). И привяжите его к аккаунту в *Личном кабинете* или нажав на кнопку ниже."},
            {answer.data_failed, "Не удалось получить данные по введенному Api. Повторите попытку через пару минут."},
            {answer.data_already_has_reciver, "Апи уже привязан к этому аккаунту." },

            {answer.unk_command, "Неизвестная команда"},
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
                        await MessageHandler(update.Message);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public async Task SendMessage(long senderId, string text, IReplyMarkup? markup = null)
        {
            if (text.Length > MaxMessageLenght)
                text = text.Remove(MaxMessageLenght);
            await botClient.SendTextMessageAsync(senderId, text, replyMarkup: markup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
        }

        public async Task SendMessage(long senderId, string text, FileStream stream, IReplyMarkup? markup = null)
        {
            if (text.Length > MaxMessageLenght)
                text = text.Remove(MaxMessageLenght);

            await botClient.SendPhotoAsync(senderId, new InputOnlineFile(stream, "photo"), text, replyMarkup: markup);
        }

        public async Task QueryHandler(CallbackQuery query)
        {
            long senderId = query.Message.Chat.Id;
            ClientData[]? caller = GetClient(senderId);

            string[] cmds = query.Data.Split(' ');
            string cmd = cmds[0];


            if (commands[cmd][0].queryCallback == null)
            {
                int page = int.Parse(cmds[1]);
                if (page == -1)
                    return;
                string text = commands[cmd][page].callback.Invoke(this, caller);

                if (text.Length > MaxMessageLenght)
                    text = text.Remove(MaxMessageLenght);

                InlineKeyboardMarkup markup = GetCommandMarkup(cmd, page);

                await botClient.EditMessageTextAsync(query.Message.Chat, query.Message.MessageId, text, replyMarkup: markup);
            }
            else
            {
                (string, InlineKeyboardMarkup)? answ = clientBook[senderId].queryCallback?.Invoke(this, caller, query);

                if (answ != null)
                {
                    await botClient.EditMessageTextAsync(query.Message.Chat, query.Message.MessageId, answ.Value.Item1, replyMarkup: answ.Value.Item2);
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
                    await SendMessage(senderId, answerList[answer.enter_api]);
                }
                else
                {
                    await SendMessage(senderId, answerList[answer.acc_exists]);
                }
            }
            else if (clientBook.ContainsKey(senderId))
            {
                if (commands.ContainsKey(message))
                {
                    ClientData[]? caller = GetClient(senderId);

                    if (caller == null)
                    {
                        //err
                        await botClient.SendTextMessageAsync(update.Chat, "");
                        return;
                    }

                    if (commands[message][0].queryCallback is not null)
                    {
                        (string, InlineKeyboardMarkup?) ans = commands[message][0].queryCallback?.Invoke(this, caller, senderId) ?? ("Не известаня команда", null);
                        await SendMessage(senderId, ans.Item1, ans.Item2);
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
            }
            else
            {
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


            Console.WriteLine($"{senderId}: {message}");
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
            }
            else
            {
                ClientData newClient = new ClientData(message, senderId);

                try
                {
                    WbBaseManager.update(message, newClient.incomeData, "incomes");
                    WbBaseManager.update(message, newClient.stocksData, "stocks");
                    WbBaseManager.update(message, newClient.salesData, "sales", "&flag=0");
                    WbBaseManager.update(message, newClient.ordersData, "orders", "&flag=0");

                    if(newClient.salesData.sales.Count > 0){
                        newClient.Name = WbBaseManager.getAccountInfo(newClient.salesData.sales.ElementAt(0).Value.nmId);
                    }
                    else if (newClient.ordersData.orders.Count > 0)
                    {
                        newClient.Name = WbBaseManager.getAccountInfo(newClient.ordersData.orders.ElementAt(0).Value.nmId);
                    }
                    else if (newClient.incomeData.incomes.Count > 0)
                    {
                        newClient.Name = WbBaseManager.getAccountInfo(newClient.incomeData.incomes.ElementAt(0).Value[0].nmId);
                    }
                    else if (newClient.stocksData.stocks.Count > 0)
                    {
                        newClient.Name = WbBaseManager.getAccountInfo(newClient.stocksData.stocks.ElementAt(0).Value[0].nmId);
                    }
                    else
                    {
                        await SendMessage(senderId, answerList[answer.data_failed]);
                        return;
                    }
                }
                catch (WbException ex)
                {
                    if (ex.exceptionType == WbException.ExceptionType.data_too_many_request)
                    {
                        await SendMessage(senderId, answerList[answer.data_too_many_requests]);
                    }else if (ex.exceptionType == WbException.ExceptionType.data_bad_request)
                    {
                        await SendMessage(senderId, answerList[answer.data_bad_request]);
                    }
                    Console.WriteLine(ex.ToString());
                    return;
                }

                new Thread(new ParameterizedThreadStart(WbBaseManager.ClientDataUpdating)).Start(new Tuple<Bot, ClientData[]>(this, new ClientData[1] { newClient }));
                clientDatas.Add(message, newClient);
                await SendMessage(senderId, answerList[answer.data_successfuly]);
            }

            if (!clientBook.ContainsKey(senderId))
            {
                clientBook.Add(senderId, new Client(message));
            }
            else
            {
                clientBook[senderId].clientDatas.Add(message);
            }
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
                    new InlineKeyboardButton(client[i].Name)
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

            string data = query.Data;
            long senderId = query.Message.Chat.Id;

            if (data == "/my +")
            {
                bot.clientBook[senderId].messageCallback = AddNewAccount;
                return ("Введите апи:", null);
            }
            else if (data == "/my <")
            {
                return GetClientAccountInfo(bot, client, senderId);
            }

            return ("/_(0_0)_/ | Редактирование профиля еще не доступно", new InlineKeyboardMarkup(new InlineKeyboardButton[] { new InlineKeyboardButton("< Назад") { CallbackData = "/my <" } }));
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


        unk_command = 128,
    }
}
