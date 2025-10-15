namespace w2ds.Utility.Types
{
    /// <summary>
    /// Extension metods for the game category enum
    /// </summary>
    public static class GameCategoryExtensions
    {
        /// <summary>
        /// Matches the game category enum with its german name
        /// </summary>
        /// <param name="category">The game category enum</param>
        /// <returns>The german term as a string</returns>
        public static string ToGermanName(this GameCategory category)
        {
            return category switch
            {
                GameCategory.Poker => "Poker",
                GameCategory.Cards => "Karten",
                GameCategory.Bingo => "Bingo und Klicksis",
                GameCategory.Virtual => "Virtual Games",
                GameCategory.Lottery => "Lotterien",
                GameCategory.Sports => "Sportwetten",
                GameCategory.Casino => "Casino Spiele",
                GameCategory.Tournaments => "Turniere",
                GameCategory.All => "",
                _ => ""
            };
        }

        /// <summary>
        /// Parses a string into a game category
        /// </summary>
        /// <param name="category">The game category as a string</param>
        /// <returns>The game category as an enum</returns>
        public static GameCategory Parse(string category)
        {
            return category.ToLower() switch
            {
                "poker" => GameCategory.Poker,
                "cards" or "karten" => GameCategory.Cards,
                "bingo" => GameCategory.Bingo,
                "virtual" => GameCategory.Virtual,
                "lottery" or "lotterien" => GameCategory.Lottery,
                "sports" or "sportwetten" => GameCategory.Sports,
                "casino" => GameCategory.Casino,
                "tournaments" or "turniere" => GameCategory.Tournaments,
                _ => GameCategory.All
            };
        }

        /// <summary>
        /// Gets all different possible game categories as a list of string
        /// </summary>
        /// <returns>A list of categories as a string</returns>
        public static List<string> GetAllCategories()
        {
            return new()
            {
                "all", "poker", "cards", "bingo", "virtual",
                "lottery", "sports", "casino", "tournaments"
            };
        }
    }
}
