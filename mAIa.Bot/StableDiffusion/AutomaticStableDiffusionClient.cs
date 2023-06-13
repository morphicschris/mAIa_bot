namespace DiscordTest.StableDiffusion
{
    using Newtonsoft.Json;
    using OpenAI_API.Images;
    using System.Net.Http.Headers;

    public class AutomaticStableDiffusionClient
    {
        private readonly HttpClient _httpClient;
        private const string API_BASE_URL = "http://127.0.0.1:7860";

        public AutomaticStableDiffusionClient()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(API_BASE_URL) };
            _httpClient.DefaultRequestHeaders.Connection.TryParseAdd("keep-alive");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36 Edg/112.0.1722.58");
            _httpClient.DefaultRequestHeaders.Referrer = new Uri("http://127.0.0.1:7860/docs");
        }

        public async Task ChangeModel(string model)
        {
            try
            {
                var jsonContent = JsonConvert.SerializeObject(new { sd_model_checkpoint = model });
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync("sdapi/v1/options", content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API Error: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calling API: {ex.Message}");
            }
        }

        public async Task<IEnumerable<SDImage>> Txt2Img(JsonSchema schema)
        {
            try
            {
                var jsonContent = JsonConvert.SerializeObject(schema);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync("sdapi/v1/txt2img", content);

                var images = new List<SDImage>();

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<Txt2ImgResponse>(responseContent);

                    if (result != null && result.images != null && result.images.Any())
                    {
                        var i = 0;
                        var info = JsonConvert.DeserializeObject<Txt2ImgInfo>(result.info);

                        foreach (var file in result.images)
                        {
                            var bytes = Convert.FromBase64String(file);

                            images.Add(new SDImage
                            {
                                Seed = info.AllSeeds[i],
                                Data = bytes
                            });

                            ++i;
                        }
                    }
                    else
                    {
                        return new List<SDImage>();
                    }

                    return images;
                }
                else
                {
                    throw new Exception($"API Error: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calling API: {ex.Message}");
            }
        }
    }
}