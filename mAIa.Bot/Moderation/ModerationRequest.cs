namespace DiscordTest.Moderation
{
    using Newtonsoft.Json;

    public class ModerationResult
    {
        [JsonProperty("categories")]
        public ModerationResultCategories Categories { get; set; }

        [JsonProperty("category_scores")]
        public ModerationResultCategoryScores CategoryScores { get; set; }

        [JsonProperty("flagged")]
        public bool Flagged { get; set; }
    }
}