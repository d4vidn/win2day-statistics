using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using w2ds.Utility.Cookies;

namespace w2ds.Utility.Services
{
    /// <summary>
    /// Simulates the login process on win2day on a test browser instance
    /// </summary>
    public static class LoginCommand
    {
        /// <summary>
        /// Executes the login command
        /// </summary>
        /// <returns>A task</returns>
        public static async Task Execute()
        {
            IWebDriver? driver = null;

            try
            {
                Console.WriteLine("Starting browser...");
                Console.WriteLine("Please login manually");

                var options = new ChromeOptions();
                options.AddArgument("--start-maximized");

                driver = new ChromeDriver(options);
                driver.Navigate().GoToUrl("https://www.win2day.at/mysettings/loginneeded");

                Console.WriteLine("The browser will wait 60 seconds before closing");

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));

                bool loginSuccess = false;

                try
                {
                    wait.Until(d =>
                    {
                        string url = d.Url.ToLower();
                        bool isLoginPage = url.Contains("loginneeded") ||
                                          url.Contains("logincaptchaneeded") ||
                                          url.Contains("perform_login");

                        return !isLoginPage;
                    });

                    loginSuccess = true;
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine("Operation has timed out - the user did not login within 60 seconds");
                    return;
                }

                if (loginSuccess)
                {
                    Console.WriteLine("Closing browser in 3 seconds...");
                    await Task.Delay(3000);

                    var cookies = driver.Manage().Cookies.AllCookies;

                    var cookieData = new CookieData();

                    foreach (var cookie in cookies)
                    {
                        cookieData.AllCookies[cookie.Name] = cookie.Value;

                        if (cookie.Name == "uepSessionId") cookieData.UepSessionId = cookie.Value;
                        else if (cookie.Name == "nm") cookieData.Nm = cookie.Value;
                        else if (cookie.Name == "fn") cookieData.Fn = cookie.Value;
                    }

                    if (string.IsNullOrEmpty(cookieData.UepSessionId))
                    {
                        Console.WriteLine("An error has occurred: Unable to find the UEP session ID");
                        return;
                    }

                    CookieManager.SaveCookies(cookieData);

                    Console.WriteLine("Login successful\n");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error has occured while trying to fetch cookies: {e.Message}");
            }
            finally
            {
                driver?.Quit();
            }
        }
    }
}