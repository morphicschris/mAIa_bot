namespace DiscordTest.Moderation
{
    using Newtonsoft.Json;

    public class ModerationResultCategories
    {
        [JsonProperty("hate")]
        public bool Hate { get; set; }

        [JsonProperty("hate/threatening")]
        public bool HateThreatening { get; set; }

        [JsonProperty("self-harm")]
        public bool SelfHarm { get; set; }

        [JsonProperty("sexual")]
        public bool Sexual { get; set; }

        [JsonProperty("sexual/minors")]
        public bool SexualMinors { get; set; }

        [JsonProperty("violence")]
        public bool Violence { get; set; }

        [JsonProperty("violence/graphic")]
        public bool ViolenceGraphic { get; set; }
    }
}