namespace DiscordTest.StableDiffusion
{
    using Discord;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class ImageQueuer
    {
        private readonly Queue<(string prompt, Guid messageId)> _queue;
        private readonly SemaphoreSlim _semaphore;

        public ImageQueuer()
        {
            _queue = new Queue<(string prompt, Guid messageId)>();
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task<IEnumerable<SDImage>> GenerateImage(string prompt, string style, Guid messageId, long seed = -1, bool hiRes = false)
        {
            //var sd = new AutomaticStableDiffusionClient();
            var sd = new ComfyStableDiffusionClient();

            var model = "galaxytimemachinesGTM_v3";

            switch (style)
            {
                case "anime": { model = "flat2DAnimerge_v10.safetensors"; break; }
                case "realistic": { model = "neurogenV11_v11.safetensors"; break; }
                default:
                case "mixed": { model = "galaxytimemachinesGTM_v3.safetensors"; break; }
            }

            await sd.ChangeModel(model);

            return await sd.Txt2Img(new JsonSchema
            {
                prompt = prompt,
                steps = 20,
                negative_prompt = "EasyNegtive, badhandv4, (bad_prompt:0.8), nsfw, watermark, nudity",
                sampler_index = "DPM++ 2M Karras",
                width = 512,
                height = 512,
                enable_hr = hiRes,
                batch_size = hiRes ? 1 : 4,
                cfg_scale = 7,
                restore_faces = false,
                denoising_strength = 0.5,
                seed = seed,
                hr_scale = 2.5D,
                hr_upscaler = "R-ESRGAN 4x+ Anime6B",
                model = model
            });
        }
    }

    public class FolderWatcher
    {
        private int _newFilesCount;
        private readonly int _maxNewFiles;
        private readonly string _folderPath;
        private readonly string _filePrefix;
        private List<byte[]> fileContents;

        public FolderWatcher(string folderPath, int maxNewFiles, string filePrefix)
        {
            _folderPath = folderPath;
            _maxNewFiles = maxNewFiles;
            _filePrefix = filePrefix;
            _newFilesCount = 0;
            fileContents = new List<byte[]>();
        }

        public async Task<IEnumerable<byte[]>> WatchAsync()
        {
            using var watcher = new FileSystemWatcher(_folderPath);

            watcher.Created += OnCreated;
            watcher.EnableRaisingEvents = true;

            while (_newFilesCount < _maxNewFiles)
            {
                await Task.Delay(1000);
            }

            return fileContents;
        }

        private async void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (File.Exists(e.FullPath) && e.Name.StartsWith(_filePrefix))
            {
                // Hacky way to get it to work
                System.Threading.Thread.Sleep(1000);

                fileContents.Add(await File.ReadAllBytesAsync(e.FullPath));

                _newFilesCount++;

            }
        }
    }

    public class SDImage
    {
        public byte[] Data { get; set; }

        public long Seed { get; set; }
    }

    public class Txt2ImgInfo
    {
        [JsonProperty("prompt")]
        public string Prompt { get; set; }

        [JsonProperty("all_prompts")]
        public List<string> AllPrompts { get; set; }

        [JsonProperty("negative_prompt")]
        public string NegativePrompt { get; set; }

        [JsonProperty("all_negative_prompts")]
        public List<string> AllNegativePrompts { get; set; }

        [JsonProperty("seed")]
        public long Seed { get; set; }

        [JsonProperty("all_seeds")]
        public List<long> AllSeeds { get; set; }

        [JsonProperty("subseed")]
        public long Subseed { get; set; }

        [JsonProperty("all_subseeds")]
        public List<long> AllSubseeds { get; set; }

        [JsonProperty("subseed_strength")]
        public double SubseedStrength { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("sampler_name")]
        public string SamplerName { get; set; }

        [JsonProperty("cfg_scale")]
        public double CfgScale { get; set; }

        [JsonProperty("steps")]
        public int Steps { get; set; }

        [JsonProperty("batch_size")]
        public int BatchSize { get; set; }

        [JsonProperty("restore_faces")]
        public bool RestoreFaces { get; set; }

        [JsonProperty("face_restoration_model")]
        public string FaceRestorationModel { get; set; }

        [JsonProperty("sd_model_hash")]
        public string SdModelHash { get; set; }

        [JsonProperty("seed_resize_from_w")]
        public int SeedResizeFromW { get; set; }

        [JsonProperty("seed_resize_from_h")]
        public int SeedResizeFromH { get; set; }

        [JsonProperty("denoising_strength")]
        public double DenoisingStrength { get; set; }

        [JsonProperty("extra_generation_params")]
        public Dictionary<string, object> ExtraGenerationParams { get; set; }

        [JsonProperty("index_of_first_image")]
        public int IndexOfFirstImage { get; set; }

        [JsonProperty("infotexts")]
        public List<string> Infotexts { get; set; }

        [JsonProperty("styles")]
        public List<object> Styles { get; set; }

        [JsonProperty("job_timestamp")]
        public string JobTimestamp { get; set; }

        [JsonProperty("clip_skip")]
        public int ClipSkip { get; set; }

        [JsonProperty("is_using_inpainting_conditioning")]
        public bool IsUsingInpaintingConditioning { get; set; }
    }

}
