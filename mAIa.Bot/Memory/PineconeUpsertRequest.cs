namespace mAIa.Bot.Memory
{
    using Newtonsoft.Json;

    public class PineconeUpsertRequest
    {
        public PineconeUpsertRequest()
        {
            Vectors = new List<Vector>();
        }

        [JsonProperty("vectors")]
        public List<Vector> Vectors { get; set; }
    }
}