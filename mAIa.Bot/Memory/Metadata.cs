namespace mAIa.Bot.Memory
{
    using Newtonsoft.Json;

    public class Metadata
    {
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("messageType")]
        public string MessageType { get; set; }
    }
}