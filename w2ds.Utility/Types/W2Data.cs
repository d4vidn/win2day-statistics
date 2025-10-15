namespace w2ds.Utility.Types
{
    /// <summary>
    /// Represents the data received from the win2day endpoint
    /// </summary>
    public class W2Data
    {
        public List<PlayedGame> Data { get; set; } = null!;

        public bool HasNextPage { get; set; }

        public string? NextIndex { get; set; }

        public string? PrevIndex { get; set; }
    }
}
