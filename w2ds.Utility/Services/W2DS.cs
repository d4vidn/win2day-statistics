using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.RegularExpressions;
using w2ds.Utility.Types;

namespace w2ds.Utility
{
    /// <summary>
    /// The main utility class to fetch game history from win2day
    /// </summary>
    public static class W2DS
    {
        private static readonly HttpClientHandler handler = new()
        {
            UseCookies = true,
            CookieContainer = new CookieContainer(),
            AllowAutoRedirect = true
        };

        private static readonly HttpClient client = new(handler);

        private static string? sessionToken;

        private const int RowsPerBatch = 500;

        static W2DS()
        {
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Accept-Language", "de-DE,de;q=0.9");
            client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36");
        }

        /// <summary>
        /// Loads cookies from the JSON file and configures the http client handler accordingly
        /// </summary>
        public static void LoadCookies()
        {
            var cookies = CookieManager.LoadCookies();

            if (cookies == null)
            {
                Console.WriteLine("No cookies were found");
                return;
            }

            var uri = new Uri("https://www.win2day.at");

            foreach (var i in cookies.AllCookies)
            {
                try
                {
                    var cookie = new Cookie(i.Key, i.Value)
                    {
                        Domain = ".win2day.at",
                        Path = "/"
                    };
                    handler.CookieContainer.Add(uri, cookie);
                }
                catch { }
            }
        }

        /// <summary>
        /// Gets the win2day user session token
        /// </summary>
        /// <returns>The win2day session token as a string</returns>
        private static async Task<string?> GetSessionToken()
        {
            if (sessionToken != null) return sessionToken;

            try
            {
                var response = await client.GetAsync("https://www.win2day.at/");
                string html = await response.Content.ReadAsStringAsync();

                // dont ask me about regex... i dont understand it either
                var match = Regex.Match(html, @"""sessionToken""\s*:\s*""([a-f0-9]{32})""", RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    sessionToken = match.Groups[1].Value;
                    Console.WriteLine($"Session token: {sessionToken}");
                    return sessionToken;
                }

                Console.WriteLine("No session token was found");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while trying to fetch the session token: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets a batch of played games from the win2day endpoint
        /// </summary>
        /// <param name="rows">The amount of rows to fetch per batch</param>
        /// <param name="nextIndex">The index from a cursor for proper pagination</param>
        /// <param name="prevIndex">The index from a cursor for proper pagination</param>
        /// <returns>A list of played games</returns>
        private static async Task<(List<PlayedGame> games, bool hasMore, string? nextIndex, string? prevIndex)?> FetchBatch(
            int rows,
            string? nextIndex = null,
            string? prevIndex = null)
        {
            try
            {
                var token = await GetSessionToken();

                if (token == null)
                {
                    Console.WriteLine("Unable to fetch data without a session token - try logging in first with the login command");
                    return null;
                }

                string url;

                if (nextIndex == null && prevIndex == null)
                {
                    url = $"https://www.win2day.at/gamehistory_json?uepSessionToken={token}&viewedPage=1&j_rows={rows}&s_onlyOpen=false";
                }
                else
                {
                    url = $"https://www.win2day.at/gamehistory_json?uepSessionToken={token}&viewedPage=1&j_rows={rows}&nextindex={nextIndex}&previndex={prevIndex}&action=next&next=next&s_onlyOpen=false";
                }

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Referer", "https://www.win2day.at/mysettings/gamehistory");
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"An error occured while trying to fetch played games - status code:{response.StatusCode}");
                    return null;
                }

                string json = await response.Content.ReadAsStringAsync();

                if (json.TrimStart().StartsWith("<"))
                {
                    Console.WriteLine("Session expired - try running the login command again");
                    return null;
                }

                var jObject = JObject.Parse(json);

                var games = jObject["data"]?.ToObject<List<PlayedGame>>() ?? [];
                var hasNextPage = jObject["hasNextPage"]?.Value<bool>() ?? false;

                string? newNextIndex = null;
                string? newPrevIndex = null;

                if (hasNextPage && games.Count > 0)
                {
                    var lastGame = games.Last();
                    var firstGame = games.First();

                    newNextIndex = lastGame.DateTimestamp?.ToString();
                    newPrevIndex = $"{newNextIndex}_{firstGame.DateTimestamp}";
                }

                return (games, hasNextPage, newNextIndex, newPrevIndex);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occured while trying to fetch played games: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the win2day game history of a user
        /// </summary>
        /// <param name="maxGames">The maximum amount of games to fetch</param>
        /// <returns>A list of played games</returns>
        public static async Task<List<PlayedGame>> GetGameHistory(int? maxGames = null)
        {
            var allGames = new List<PlayedGame>();
            string? nextIndex = null;
            string? prevIndex = null;
            int batchCount = 0;

            Console.WriteLine("Fetching game history...");

            while (true)
            {
                batchCount++;

                int rowsToFetch = RowsPerBatch;

                if (maxGames.HasValue)
                {
                    int remaining = maxGames.Value - allGames.Count;
                    if (remaining <= 0) break;
                    rowsToFetch = Math.Min(RowsPerBatch, remaining);
                }

                var result = await FetchBatch(rowsToFetch, nextIndex, prevIndex);

                if (result == null)
                {
                    Console.WriteLine("Failed to fetch batch");
                    break;
                }

                var (games, hasMore, newNextIndex, newPrevIndex) = result.Value;

                if (games.Count == 0) break;

                allGames.AddRange(games);

                if (maxGames.HasValue && allGames.Count >= maxGames.Value)
                {
                    allGames = allGames.Take(maxGames.Value).ToList();
                    break;
                }

                if (!hasMore)
                {
                    Console.WriteLine("\nNo more pages available");
                    break;
                }

                nextIndex = newNextIndex;
                prevIndex = newPrevIndex;

                await Task.Delay(300);
            }

            Console.WriteLine($"Total games fetched: {allGames.Count}");

            return allGames;
        }
    }
}