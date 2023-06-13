namespace DiscordTest.StableDiffusion
{
    using Newtonsoft.Json;
    using System.Net.Http.Headers;

    public class ComfyStableDiffusionClient
    {
        private readonly HttpClient _httpClient;
        private const string API_BASE_URL = "http://127.0.0.1:8188/";

        public ComfyStableDiffusionClient()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(API_BASE_URL) };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Random = new Random();
        }

        protected virtual Random Random { get; }

        public async Task ChangeModel(string model)
        {
        }

        public async Task<IEnumerable<SDImage>> Txt2Img(JsonSchema schema)
        {
            var images = new List<SDImage>();

            try
            {
                for (int i = 0; i < schema.batch_size; i++)
                {
                    var jobNumber = Guid.NewGuid().ToString();
                    var folderWatcher = new FolderWatcher(@"D:\Git\ComfyUI_windows_portable\ComfyUI\output\mAIa\", 1, jobNumber);
                    var seed = schema.seed;

                    if (seed == -1)
                    {
                        seed = Random.Next(int.MaxValue);
                    }

                    var jsonContent = JsonConvert.SerializeObject(schema);
                    var file = schema.enable_hr ? "upscale" : "txt2img";
                    var promptSchema = File.ReadAllText($@"StableDiffusion\{file}.json");;

                    promptSchema = promptSchema
                        .Replace("{{seed}}", seed.ToString())
                        .Replace("{{checkpoint}}", schema.model)
                        .Replace("{{prompt}}", schema.prompt)
                        .Replace("{{jobnumber}}", jobNumber);

                    var content = new StringContent(promptSchema, System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await _httpClient.PostAsync("prompt", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"API Error: {response.ReasonPhrase}");
                    }

                    var data = await folderWatcher.WatchAsync();

                    var image = new SDImage
                    {
                        Seed = seed,
                        Data = data.First()
                    };

                    images.Add(image);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calling API: {ex.Message}");
            }

            return images;
        }
    }
}