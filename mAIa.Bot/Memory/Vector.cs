namespace mAIa.Bot.Memory
{
    using Newtonsoft.Json;

    public class Vector
    {
        public Vector()
        {
            Values = new float[0];
            Metadata = new Metadata();
        }

        [JsonProperty("values")]
        public float[] Values { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}