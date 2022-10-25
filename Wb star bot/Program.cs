using System.IO;
using Wb_star_bot.Telegram_Bot;
using Wb_star_bot.Clients;
using Wb_star_bot.Wb_handler;
using Wb_star_bot;
using System.Net;






int x = int.Parse(Console.ReadLine());
int y = int.Parse(Console.ReadLine());

int[,] X = new int[x, y];

for (int i = 0; i < y; i++) {
    string[] s1 = Console.ReadLine().Split(" ");
    for(int j = 0; j < s1.Length; j++)
    {
        X[j, i] = int.Parse(s1[j]);
    }
}
Console.WriteLine("");

for (int i = 0; i < X.GetLength(0); i++)
{
    for(int j = 0; j < X.GetLength(1); j++)
    {
        if (X[i,j] == 2)
        {
            int[,] ints = Check(i, j, X).Item2;

            for (int xc = 0; xc < ints.GetLength(0); xc++)
            {
                for(int yc = 0; yc < ints.GetLength(1); yc++)
                {
                    Console.Write(ints[yc, xc].ToString() + " ");
                }
                Console.WriteLine("");
            }
        }
    }
}

Console.ReadLine();

(int,int[,]) Check(int x, int y, int[,] X, int count = 0)
{
    if (x < 0 || y < 0 || x >= X.GetLength(0) || y >= X.GetLength(1) || X[x,y] == 1 || X[x,y] == 4 || count == -1)
        return (-1, null);

    if (X[x, y] == 3)
        return (count, X);

    int[,] newX = new int[X.GetLength(0), X.GetLength(1)];

    for (int i = 0; i < newX.GetLength(0); i++)
    {
        for (int j = 0; j < newX.GetLength(1); j++)
        {
            newX[i, j] = X[i, j];
        }
    }
    newX[x, y] = 4;

    count++;

    (int,int[,])[] ways = new (int, int[,])[4];

    ways[0] = Check(x - 1, y, newX, count);
    ways[1] = Check(x + 1, y, newX, count);
    ways[2] = Check(x, y - 1, newX, count);
    ways[3] = Check(x, y + 1, newX, count);

    (int, int[,]) w = ways[0];

    for(int i = 0; i < ways.Length; i++)
    {
        if (ways[i].Item1 == -1)
        {
            continue;
        }

        if (ways[i].Item1 < w.Item1 || w.Item1 == -1)
        {
            w = ways[i];
        }
    }

    return w;
}















return;
string token = "5719447713:AAF-7w3jQQnvs2v9ZjzJ-5nEL61fzYD0n8M";

if (!Directory.Exists(WbBaseManager.output))
{
    Directory.CreateDirectory(WbBaseManager.output);
}

Console.WriteLine($"Current bot files directory: {WbBaseManager.output}");


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
        case "end": await bot.QuitMessage(); return;

    }
}