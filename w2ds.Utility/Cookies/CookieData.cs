namespace w2ds.Utility.Cookies
{
    /// <summary>
    /// Represents all cookies found during the win2day login process
    /// </summary>
    public class CookieData
    {
        public Dictionary<string, string> AllCookies { get; set; } = []
        ;
        public DateTime SavedAt { get; set; }

        public string UepSessionId { get; set; } = "";

        public string Nm { get; set; } = "";

        public string Fn { get; set; } = "";
    }
}