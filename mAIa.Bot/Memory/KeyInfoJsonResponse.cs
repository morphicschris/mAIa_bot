namespace mAIa.Bot.Memory
{
    using Newtonsoft.Json;

    public class KeyInfoJsonResponse
    {
        [JsonProperty("keyInfo")]
        public KeyInfo[] KeyInfo { get; set; }
    }
}