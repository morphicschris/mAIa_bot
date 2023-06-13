namespace DiscordTest.Moderation
{
    using Newtonsoft.Json;
    using System.Net.Http.Headers;
    using System.Text;

    public class ModerationHandler
    {
        private readonly string _apiKey;
        private readonly string _moderationEndpoint = "https://api.openai.com/v1/moderations";

        public ModerationHandler(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<bool> IsAppropriateAsync(string content)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var requestContent = new StringContent(JsonConvert.SerializeObject(new { input = content }), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(_moderationEndpoint, requestContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var moderationResult = JsonConvert.DeserializeObject<ModerationResponse>(responseContent);

                    return moderationResult.Results.All(e => !e.Flagged);
                }
            }

            throw new Exception("Failed to moderate content.");
        }
    }
}