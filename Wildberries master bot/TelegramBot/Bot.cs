using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;


namespace Wildberries_master_bot.TelegramBot
{

    internal class Bot
    {
        public const int MaxMessageLenght = 2048; // tg limit is 4096

        public static Dictionary<answer, string> answers = new Dictionary<answer, string>()
        {
            {answer.start, "Используйте команду /start"},
            {answer.enter_api, "Введите апи"},
            {answer.error_api, "Апи введено некорректно"},
            {answer.acc_exists, "Вы уже зарегестрированны"},
            {answer.data_bad_request, "Не удалось привязать апи. Пороверьте апи, в противном случае перевыпустите новый апи ключ"},
            {answer.data_too_many_requests, "Сервисы Wildberries сейчас не доступны. Повторите попытку через пару минут."},
            {answer.data_successfuly, "Апи успешно привязан!"},


            {answer.unk_command, "Неизвестная команда"},
        };


        public Dictionary<long, ClientData> botClients;

        public Dictionary<string, BotAction[][]> botPages;

        public bool showMessages = false;

        public Bot(string token, Dictionary<string, BotAction[][]> botPages, string? clients = null)
        {
            ITelegramBotClient bot = new TelegramBotClient(token);

            if (clients == null)
            {
                botClients = new Dictionary<long, ClientData>();
            }
            else
            {
                botClients = JsonConvert.DeserializeObject<Dictionary<long, ClientData>>(clients) ?? new Dictionary<long, ClientData>();
                Console.WriteLine("Клиенты загружены успешно!");
            }

            this.botPages = botPages;

            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            bot.StartReceiving(
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
            switch (update.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                    await MessageQueryHandler(botClient, update.CallbackQuery);
                    break;
                case Telegram.Bot.Types.Enums.UpdateType.Message:
                    var message = update.Message;
                    string msg = message?.Text ?? "null";

                    if (message == null || msg == "null") return;

                    long senderId = message.Chat.Id;

                    if (showMessages)
                        Console.WriteLine($"{senderId}: {msg}");

                    await answer();

                    async Task answer()
                    {
                        ClientData? data = (botClients.ContainsKey(senderId) ? botClients[senderId] : null);

                        (string text, IReplyMarkup? keys) result = MessageHander(data, msg, senderId).Result;

                        if (result.text.Length > MaxMessageLenght)
                        {
                            result.text = result.text.Remove(MaxMessageLenght) + "...";
                        }
                        if (result.keys == null)
                        {
                            await botClient.SendTextMessageAsync(message.Chat, result.text);
                        }
                        else
                        {
                            await botClient.DeleteMessageAsync(message.Chat, message.MessageId);
                            await botClient.SendTextMessageAsync(message.Chat, result.text, replyMarkup: result.keys);
                        }
                    }
                    break;
            }
        }

        public async Task ErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception);
        }

        public async Task MessageQueryHandler(ITelegramBotClient botClient,  CallbackQuery? query)
        {
            if (query == null)
                return;

            ClientData data = botClients[query.Message.Chat.Id];

            if (data == null) return;

            string[] args = query.Data.Split(' ');
            int y = int.Parse(args[1]);
            int x = int.Parse(args[2]);

            var action = botPages[args[0]][y][x].action(data);

            await botClient.EditMessageTextAsync(query.Message.Chat, query.Message.MessageId, action);
        }

        public async Task<(string, IReplyMarkup?)> MessageHander(ClientData? data, string message, long senderId)
        {
            answer? base_cmds = CheckBaseCommands(message, senderId);

            if (base_cmds is not null)
            {
                return (answers[base_cmds ?? answer.unk_command], null);
            }

            if (data is not null)
            {
                if (data.apiKey is null)
                {
                    return (answers[UpdateClientApi(data, message)], null);
                }
                else
                {
                    if (message.StartsWith("/"))
                    {
                        if (botPages.ContainsKey(message))
                        {
                            return (data.GetData(message), MessageButtons(message, botPages[message]));
                        }
                    }
                    else
                    {
                        
                    }
                }
            }
            else
            {
                return (answers[answer.start], null);
            }
            return (answers[answer.unk_command], null);
        }

        public answer? CheckBaseCommands(string message, long senderId)
        {
            if (message == "/start")
            {
                if (botClients.ContainsKey(senderId))
                {
                    return answer.acc_exists;
                }

                botClients.Add(senderId, new ClientData());

                return answer.enter_api;
            }
            return null;
        }

        public answer UpdateClientApi(ClientData data, string api)
        {
            if (api.Length != 48 || api.Contains(' '))
            {
                return answer.error_api;
            }
            else
            {
                data.apiKey = api;
                try
                {
                    WbHandler.ClientDataUpdate(data);
                }
                catch (WbException e)
                {
                    data.apiKey = null;

                    switch (e.exceptionType)
                    {
                        case WbException.ExceptionType.data_bad_request:
                            return answer.data_bad_request;
                        case WbException.ExceptionType.data_too_many_request:
                            return answer.data_too_many_requests;
                    }
                }
            }
            return answer.data_successfuly;
        }

        public InlineKeyboardMarkup MessageButtons(string key, BotAction[][] actions)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[actions.Length][];
            for (int y = 0; y < actions.Length; y++)
            {
                buttons[y] = new InlineKeyboardButton[actions[y].Length];
                for (int x = 0; x < actions[y].Length; x++)
                {
                    buttons[y][x] = new InlineKeyboardButton(actions[y][x].command)
                    {
                        CallbackData = $"{key} {y} {x}"
                    };
                }
            }
            return new InlineKeyboardMarkup(buttons);
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


            unk_command = 128,
        }
    }
}
