using System.Text.Json;
using w2ds.Utility.Cookies;

namespace w2ds.Utility
{
    /// <summary>
    /// Manages all cookies used within the application
    /// </summary>
    public static class CookieManager
    {
        private const string CookieFile = "win2day_cookies.json";

        /// <summary>
        /// Saves all cookies found to a JSON file
        /// </summary>
        /// <param name="cookies">The cookie data</param>
        public static void SaveCookies(CookieData cookies)
        {
            cookies.SavedAt = DateTime.Now;

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(cookies, options);
            File.WriteAllText(CookieFile, json);

            Console.WriteLine($"{cookies.AllCookies.Count} cookies were saved");
        }

        /// <summary>
        /// Loads all cookies from cookie JSON file
        /// </summary>
        /// <returns>The cookie data</returns>
        public static CookieData? LoadCookies()
        {   
            if (!File.Exists(CookieFile)) return null;

            try
            {
                string json = File.ReadAllText(CookieFile);
                var cookies = JsonSerializer.Deserialize<CookieData>(json);

                return cookies;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Deletes the win2day cookie file
        /// </summary>
        public static void DeleteCookies()
        {
            if (File.Exists(CookieFile))
            {
                File.Delete(CookieFile);
                Console.WriteLine("All cookies were deleted");
            }
        }
    }
}