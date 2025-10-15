using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using w2ds.Utility.Types;

namespace w2ds.Utility.Services
{
    /// <summary>
    /// Creates excel reports based on the game history and the game statistics
    /// </summary>
    public static class ExcelExporter
    {
        static ExcelExporter()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Creates an excel report based on the game history
        /// </summary>
        /// <param name="games"></param>
        /// <returns>The excel report filename/returns>
        public static string CreateExcelReport(List<PlayedGame> games)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = $"w2ds_{timestamp}.xlsx";

            var statistics = CalculateStatistics(games);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Game Statistics");

                // Headers
                worksheet.Cells[1, 1].Value = "Game Name";
                worksheet.Cells[1, 2].Value = "Type/Category";
                worksheet.Cells[1, 3].Value = "First Time Played";
                worksheet.Cells[1, 4].Value = "Last Time Played";
                worksheet.Cells[1, 5].Value = "Total Times Played";
                worksheet.Cells[1, 6].Value = "Total Bets";
                worksheet.Cells[1, 7].Value = "Total Wins";
                worksheet.Cells[1, 8].Value = "Biggest Bet";
                worksheet.Cells[1, 9].Value = "Biggest Loss";
                worksheet.Cells[1, 10].Value = "Biggest Win";
                worksheet.Cells[1, 11].Value = "Profit";
                worksheet.Cells[1, 12].Value = "RTP %";

                // Actual data
                int row = 2;
                foreach (var stat in statistics.OrderByDescending(s => s.TotalBets))
                {
                    worksheet.Cells[row, 1].Value = stat.GameName;
                    worksheet.Cells[row, 2].Value = stat.Category;
                    worksheet.Cells[row, 3].Value = stat.FirstPlayed.ToString("dd.MM.yyyy HH:mm");
                    worksheet.Cells[row, 4].Value = stat.LastPlayed.ToString("dd.MM.yyyy HH:mm");
                    worksheet.Cells[row, 5].Value = stat.TimesPlayed;
                    worksheet.Cells[row, 6].Value = stat.TotalBets;
                    worksheet.Cells[row, 7].Value = stat.TotalWins;
                    worksheet.Cells[row, 8].Value = stat.BiggestBet;
                    worksheet.Cells[row, 9].Value = stat.BiggestLoss;
                    worksheet.Cells[row, 10].Value = stat.BiggestWin;
                    worksheet.Cells[row, 11].Value = stat.Profit;
                    worksheet.Cells[row, 12].Value = stat.RTP;

                    // Format money columns (6-11)
                    for (int col = 6; col <= 11; col++)
                    {
                        worksheet.Cells[row, col].Style.Numberformat.Format = "€#,##0.00";
                    }

                    // Format RTP as percentage
                    worksheet.Cells[row, 12].Style.Numberformat.Format = "0.00%";

                    // Color profit
                    if (stat.Profit > 0)
                        worksheet.Cells[row, 11].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                    else if (stat.Profit < 0)
                        worksheet.Cells[row, 11].Style.Font.Color.SetColor(System.Drawing.Color.Red);

                    if (stat.RTP >= 1.0m)  // ← FIX: >= 1.0 (nicht >= 100)
                        worksheet.Cells[row, 12].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                    else
                        worksheet.Cells[row, 12].Style.Font.Color.SetColor(System.Drawing.Color.Red);

                    // Color biggest loss (always red if > 0)
                    if (stat.BiggestLoss > 0)
                        worksheet.Cells[row, 9].Style.Font.Color.SetColor(System.Drawing.Color.Red);

                    row++;
                }

                int summaryRow = row;
                worksheet.Cells[summaryRow, 1].Value = "TOTAL";
                worksheet.Cells[summaryRow, 1].Style.Font.Bold = true;
                worksheet.Cells[summaryRow, 5].Value = statistics.Sum(s => s.TimesPlayed);
                worksheet.Cells[summaryRow, 6].Value = statistics.Sum(s => s.TotalBets);
                worksheet.Cells[summaryRow, 7].Value = statistics.Sum(s => s.TotalWins);
                worksheet.Cells[summaryRow, 8].Value = statistics.Max(s => s.BiggestBet);
                worksheet.Cells[summaryRow, 9].Value = statistics.Max(s => s.BiggestLoss);
                worksheet.Cells[summaryRow, 10].Value = statistics.Max(s => s.BiggestWin);

                decimal totalBets = statistics.Sum(s => s.TotalBets);
                decimal totalWins = statistics.Sum(s => s.TotalWins);
                decimal totalProfit = totalWins - totalBets;
                decimal totalRTP = totalBets > 0 ? totalWins / totalBets : 0;

                worksheet.Cells[summaryRow, 11].Value = totalProfit;
                worksheet.Cells[summaryRow, 12].Value = totalRTP;

                // Formatting and styling
                for (int col = 6; col <= 11; col++)
                {
                    worksheet.Cells[summaryRow, col].Style.Numberformat.Format = "€#,##0.00";
                    worksheet.Cells[summaryRow, col].Style.Font.Bold = true;
                }

                worksheet.Cells[summaryRow, 12].Style.Numberformat.Format = "0.00%";
                worksheet.Cells[summaryRow, 12].Style.Font.Bold = true;

                if (totalProfit > 0)
                    worksheet.Cells[summaryRow, 11].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                else if (totalProfit < 0)
                    worksheet.Cells[summaryRow, 11].Style.Font.Color.SetColor(System.Drawing.Color.Red);

                if (totalRTP >= 1.0m)
                    worksheet.Cells[summaryRow, 12].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                else
                    worksheet.Cells[summaryRow, 12].Style.Font.Color.SetColor(System.Drawing.Color.Red);

                using (var range = worksheet.Cells[summaryRow, 1, summaryRow, 12])
                {
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
                    range.Style.Border.Top.Style = ExcelBorderStyle.Double;
                }

                var dataRange = worksheet.Cells[1, 1, row - 1, 12];
                var table = worksheet.Tables.Add(dataRange, "GameStatistics");
                table.TableStyle = TableStyles.Medium2;

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                FileInfo file = new FileInfo(filename);
                package.SaveAs(file);
            }

            return filename;
        }

        /// <summary>
        /// Calculates the player statistics based on the played games
        /// </summary>
        /// <param name="games">The played games</param>
        /// <returns>A list of game statistics</returns>
        private static List<GameStatistics> CalculateStatistics(List<PlayedGame> games)
        {
            var grouped = games.GroupBy(g => g.Game);

            var statistics = new List<GameStatistics>();

            foreach (var group in grouped)
            {
                var gameList = group.ToList();

                decimal biggestLoss = 0;
                foreach (var game in gameList)
                {
                    decimal bet = ParseMoney(game.Stake);
                    decimal win = ParseMoney(game.Prize);
                    decimal loss = bet - win;

                    if (loss > biggestLoss)
                        biggestLoss = loss;
                }

                var stat = new GameStatistics
                {
                    GameName = group.Key,
                    Category = gameList.First().GameGroup,
                    TimesPlayed = gameList.Count,
                    FirstPlayed = ParseDate(gameList.Last().Date),
                    LastPlayed = ParseDate(gameList.First().Date),
                    TotalBets = gameList.Sum(g => ParseMoney(g.Stake)),
                    TotalWins = gameList.Sum(g => ParseMoney(g.Prize)),
                    BiggestBet = gameList.Max(g => ParseMoney(g.Stake)),
                    BiggestLoss = biggestLoss,
                    BiggestWin = gameList.Max(g => ParseMoney(g.Prize))
                };

                statistics.Add(stat);
            }

            return statistics;
        }

        /// <summary>
        /// Helper method to properly parse money to decimal values
        /// </summary>
        /// <param name="moneyString">The money as a string</param>
        /// <returns>The money as a decimal</returns>
        private static decimal ParseMoney(string moneyString)
        {
            if (string.IsNullOrWhiteSpace(moneyString))
                return 0;

            string cleaned = moneyString
                .Replace("€", "")
                .Replace(" ", "")
                .Replace(",", ".")
                .Trim();

            if (decimal.TryParse(cleaned, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal result))
                return result;

            return 0;
        }

        /// <summary>
        /// Helper method to properly parse string to DateTime structs
        /// </summary>
        /// <param name="dateString">The date as a string</param>
        /// <returns>The date as a DateTime struct</returns>
        private static DateTime ParseDate(string dateString)
        {
            if (DateTime.TryParseExact(dateString, "dd.MM.yyyy HH:mm:ss",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime result))
                return result;

            return DateTime.MinValue;
        }
    }
}