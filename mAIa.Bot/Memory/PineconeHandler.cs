namespace mAIa.Bot.Memory
{
    using DiscordTest;
    using mAIa.Bot.Chat;
    using Newtonsoft.Json;
    using System.Text;

    public class PineconeHandler
    {
        public PineconeHandler(string apiKey)
        {
            ApiKey = apiKey;
        }

        protected virtual string ApiKey { get; }

        public async Task UpsertAsync(
            float[] vectors,
            string id,
            DateTime timestamp,
            ChatMessageType messageType = ChatMessageType.User,
            string username = null,
            int? userId = null)
        {
            try
            {
                var request = new PineconeUpsertRequest();
                var vector = new Vector();

                vector.Values = vectors;
                vector.Id = id;
                vector.Metadata.Username = username;
                vector.Metadata.UserId = userId.ToString();
                vector.Metadata.Timestamp = timestamp.ToString("yyyy-MM-dd hh:mm:ss");
                vector.Metadata.MessageType = messageType.ToString();

                request.Vectors.Add(vector);

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Api-Key", ApiKey);
                    client.DefaultRequestHeaders.Add("accept", "application/json");

                    string jsonString = JsonConvert.SerializeObject(request);
                    HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("https://maia-1a3d79a.svc.us-central1-gcp.pinecone.io/vectors/upsert", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception("Could not write vectors to Pinecone");
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleWriter.WriteLine(ex.Message, LogLevel.Error, ConsoleColor.Red);
            }
        }
    }
}