namespace w2ds.Utility.Types
{
    /// <summary>
    /// Represents a game played on win2day
    /// </summary>
    public class PlayedGame
    {
        public int Id { get; set; }

        public string ImageSrc { get; set; } = "";

        public string Game { get; set; } = "";

        public string Date { get; set; } = "";

        public object DateTimestamp { get; set; } = "";

        public string GameGroup { get; set; } = "";

        public string Stake { get; set; } = "";

        public string Prize { get; set; } = "";

        public string ParticipationUntilDisplay { get; set; } = "";

        public bool OpenInModal { get; set; }

        public string DetailLink { get; set; } = "";

        public string GameActionUrl { get; set; } = "";

        public string GameActionButtonLinkText { get; set; } = "";
    }
}
