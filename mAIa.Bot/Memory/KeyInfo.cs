namespace mAIa.Bot.Memory
{
    using Newtonsoft.Json;

    public class KeyInfo
    {
        [JsonProperty("category")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("relevanceToUser")]
        public string Reasoning { get; set; }

        [JsonProperty("relevant")]
        public bool Relevant { get; set; }
    }
}