using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;


namespace Wildberries_master_bot
{
    internal class Bot
    {
        public Dictionary<long, ClientData> botClients;

        public BotPage[] botPages;

        public Bot(string token, BotPage[] botPages)
        {
            ITelegramBotClient bot = new TelegramBotClient(token);
            botClients = new Dictionary<long, ClientData>();

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
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                string msg = message?.Text ?? "null";

                if (message == null || msg == "null") return;

                long senderId = message.Chat.Id;
                Console.WriteLine(msg);

                await answer();

                async Task answer()
                {
                    (string text, IReplyMarkup? keys) result = MessageHander(msg, senderId).Result;

                    await botClient.SendTextMessageAsync(message.Chat, result.text, replyMarkup: result.keys);
                }
            }
        }

        public async Task ErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        public async Task<(string, IReplyMarkup?)> MessageHander(string message, long senderId)
        {
            switch (message)
            {
                case "/start":
                    botClients.Add(senderId, new ClientData(-1));
                    return ("Введите Api64. (линк на документацию)", null);
                default:
                    if (botClients.ContainsKey(senderId))
                    {
                        ClientData data = botClients[senderId];

                        if (data.apiKey == "")
                        {
                            if (message.Length != 48 || message.Contains(' '))
                                return ("Проверьте правильность написания Api64. (линк на документацию)", null);
                            else
                            {
                                data.apiKey = message;
                                return ("Апи успешно привязан, чтобы изменить api, используйте команду (команда)", null);
                            }
                        }
                        else
                        {
                            if (data.loadedPage == -1)
                            {
                                for(int i = 0; i < botPages.Length; i++)
                                {
                                    if(message == botPages[i].PageCommand)
                                    {
                                        data.loadedPage = i;
                                        return (botPages[i].PageMessage, PageButtons(i));
                                    }
                                }
                            }
                            else
                            {
                                foreach (BotAction action in botPages[data.loadedPage].Actions)
                                {
                                    if (message == action.command)
                                    {
                                        data.loadedPage = 0;
                                        try
                                        {
                                            return (action.action(data), null);
                                        }catch(Exception e)
                                        {
                                            return ("Слишком частые запросы", null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        return ("Используйте команду /start", null);
                    }
                    break;
            }
            return ("Упс, что-то пошло не так...", null);
        }

        public ReplyKeyboardMarkup PageButtons(int pageNum)
        {
            BotPage currentPage = botPages[pageNum];

            KeyboardButton[] buttons = new KeyboardButton[currentPage.Actions.Length];

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = new KeyboardButton(currentPage.Actions[i].command);
            }

            return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true};
        }
    }
}
