namespace w2ds.Utility.Types
{
    /// <summary>
    /// Represents the excel table structure
    /// </summary>
    public class GameStatistics
    {
        public string GameName { get; set; } = "";

        public string Category { get; set; } = "";

        public DateTime FirstPlayed { get; set; }

        public DateTime LastPlayed { get; set; }

        public int TimesPlayed { get; set; }

        public decimal TotalBets { get; set; }

        public decimal TotalWins { get; set; }

        public decimal BiggestBet { get; set; }

        public decimal BiggestLoss { get; set; }

        public decimal BiggestWin { get; set; }

        public decimal Profit => TotalWins - TotalBets;

        public decimal RTP => TotalBets > 0 ? TotalWins / TotalBets : 0;
    }
}