using System.CommandLine;
using w2ds.Utility;
using w2ds.Utility.Services;
using w2ds.Utility.Types;

namespace w2ds
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("W2DS - win2day statistics");

            var loginCommand = new Command("login", "Opens a browser for the user to log into win2day");
            loginCommand.SetHandler(async () =>
            {
                await LoginCommand.Execute();
            });

            var historyCommand = new Command("history", "Fetches game history and creates Excel report");

            var limitOption = new Option<int?>(
                name: "-limit",
                getDefaultValue: () => null,
                description: "Maximum number of games to fetch (fetches all games by default)");

            var categoryOption = new Option<string>(
                name: "-category",
                getDefaultValue: () => "all",
                description: "Filter by category (all, poker, cards, bingo, virtual, lottery, sports, casino, tournaments)");

            historyCommand.AddOption(limitOption);
            historyCommand.AddOption(categoryOption);

            historyCommand.SetHandler(async (limit, categoryStr) =>
            {
                var cookies = CookieManager.LoadCookies();
                if (cookies == null)
                {
                    Console.WriteLine("No cookies found - run the login command first");
                    return;
                }

                W2DS.LoadCookies();

                var category = GameCategoryExtensions.Parse(categoryStr);

                var allGames = await W2DS.GetGameHistory(limit);

                if (allGames.Count == 0)
                {
                    Console.WriteLine("No games found");
                    return;
                }

                List<PlayedGame> games;
                if (category != GameCategory.All)
                {
                    string categoryFilter = category.ToGermanName();
                    games = allGames.Where(g => g.GameGroup == categoryFilter).ToList();
                }
                else
                {
                    games = allGames;
                }

                if (games.Count == 0)
                {
                    Console.WriteLine($"No games found in category '{category}'");
                    return;
                }

                foreach (var game in games)
                {
                    Console.WriteLine($"{game.Date} | {game.Game} | {game.Stake} -> {game.Prize}");
                }

                Console.WriteLine($"{games.Count} games were found");

                Console.WriteLine("\nCreating Excel report...");

                string filename = ExcelExporter.CreateExcelReport(games);

                Console.WriteLine($"Excel report created: {filename}");
            }, limitOption, categoryOption);

            var categoriesCommand = new Command("categories", "List all available categories");
            categoriesCommand.SetHandler(() =>
            {
                Console.WriteLine("Available categories:");
                foreach (var cat in GameCategoryExtensions.GetAllCategories())
                {
                    Console.WriteLine($"  - {cat}");
                }
            });

            var logoutCommand = new Command("logout", "Deletes win2day related cookies");
            logoutCommand.SetHandler(() =>
            {
                CookieManager.DeleteCookies();
            });

            rootCommand.AddCommand(loginCommand);
            rootCommand.AddCommand(historyCommand);
            rootCommand.AddCommand(categoriesCommand);
            rootCommand.AddCommand(logoutCommand);

            return await rootCommand.InvokeAsync(args);
        }
    }
}