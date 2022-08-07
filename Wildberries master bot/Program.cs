using Wildberries_master_bot;

string botToken = "5543050011:AAHA7XVjqTiiiBB4xmixfZNQih1S0l19rLc";

Bot bot = new Bot(botToken, new BotPage[]
{
    new BotPage("/income", "Выберите период")
    {
        Actions = new BotAction[]
        {
            new BotAction("Сегодня", WbHandler.IncomeToday),
            new BotAction("Текущий месяц", WbHandler.IncomeMounth),
        }
    }
});

Console.ReadKey();