namespace DiscordTest.Moderation
{
    using Newtonsoft.Json;

    public class ModerationResultCategoryScores
    {
        [JsonProperty("hate")]
        public float Hate { get; set; }

        [JsonProperty("hate/threatening")]
        public float HateThreatening { get; set; }

        [JsonProperty("self-harm")]
        public float SelfHarm { get; set; }

        [JsonProperty("sexual")]
        public float Sexual { get; set; }

        [JsonProperty("sexual/minors")]
        public float SexualMinors { get; set; }

        [JsonProperty("violence")]
        public float Violence { get; set; }

        [JsonProperty("violence/graphic")]
        public float ViolenceGraphic { get; set; }
    }
}