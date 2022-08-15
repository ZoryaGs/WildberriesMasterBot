using Wildberries_master_bot;
using Wildberries_master_bot.TelegramBot;
using Wildberries_master_bot.DataTypes;
using Newtonsoft.Json;
using System.IO;

string botToken = "5543050011:AAHA7XVjqTiiiBB4xmixfZNQih1S0l19rLc";

string clientsPath = "clients.txt";

Bot bot = new Bot(botToken, new Dictionary<string, BotAction[][]>
{
    {"/income", new BotAction[][]{
        new BotAction[] {
            new BotAction("Сегодня", WbHandler.IncomeToday),
            new BotAction("Текущий месяц", WbHandler.IncomeMounth),
        },
        new BotAction[] {
            new BotAction("Вчера", WbHandler.IncomeLastToday),
            new BotAction("Прошлый месяц", WbHandler.IncomeLastMounth),
        },
        new BotAction[] {
            new BotAction("Выписка за все время", null),
        },
    }},
    {"/orders", new BotAction[][]{
        new BotAction[] {
            new BotAction("Сегодня", WbHandler.OrdersToday),
            new BotAction("Текущий месяц", WbHandler.OrdersMounth),
        },
        new BotAction[] {
            new BotAction("Вчера", WbHandler.OrdersLastToday),
            new BotAction("Прошлый месяц", WbHandler.OrdersLastMounth),
        },
        new BotAction[] {
            new BotAction("Выписка за все время", null),
        },
    }},

}, GetClients());

while (true)
{
    string? cmd = Console.ReadLine();

    if (cmd != null)
    {
        switch (cmd)
        {
            case "/help":
                Console.WriteLine("/show - показывать сообщения");
                Console.WriteLine("/clear clients - удалить всех клиентов");
                Console.WriteLine("/clear - отчистить консоль");
                Console.WriteLine("/save - сохранить данные");
                Console.WriteLine("/end - сохранить данные и выйти");
                break;
            case "/show":
                Console.WriteLine("[T/F] - отоброжать/скрыть сообщения");
                string? answ = Console.ReadLine();

                if (answ != null)
                {
                    if (answ == "T")
                    {
                        bot.showMessages = true;
                    }
                    else if (answ == "F")
                    {
                        bot.showMessages = false;
                    }
                }

                break;
            case "test":
                WbHandler.IncomeLastToday(new ClientData() { apiKey = "MDgyYjIxYzktODY3ZS00N2VkLTlmMGItZjIzZTNlODNlMmUw" });
                break;
            case "/clear clients":
                bot.botClients = new Dictionary<long, ClientData>();
                if (File.Exists(clientsPath))
                {
                    File.Delete(clientsPath);
                    Console.WriteLine("Клиенты удалены успешно!");
                }
                break;
            case "/clear":
                Console.Clear();
                break;
            case "/save":
                SaveClients(bot);
                Console.WriteLine("Клиенты сохранены успешно!");
                break;
            case "/end":
                SaveClients(bot);
                Console.WriteLine("Клиенты сохранены успешно! Завершение работы...");
                return;
            default:
                Console.WriteLine("Неизвестная команда, используйте команду /help");
                break;
        }
    }
}

void SaveClients(Bot bot)
{
    if (bot.botClients.Count == 0) return;

    File.WriteAllText("clients.txt", JsonConvert.SerializeObject(bot.botClients));
}

string? GetClients()
{
    if (File.Exists(clientsPath))
    {
        return File.ReadAllText(clientsPath);
    }
    return null;
}