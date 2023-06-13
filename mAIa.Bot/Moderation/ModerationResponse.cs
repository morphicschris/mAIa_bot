namespace DiscordTest.Moderation
{
    using Newtonsoft.Json;
    using OpenAI_API.Moderation;

    public class ModerationResponse
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("results")]
        public ModerationResult[] Results { get; set; }
    }
}