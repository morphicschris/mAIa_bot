namespace DiscordTest.Commands.Handlers
{
    using Discord;
    using Discord.WebSocket;
    using DiscordTest.Moderation;
    using DiscordTest.StableDiffusion;
    using mAIa.Bot.Chat;
    using mAIa.Bot.Moderation;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using System.Threading.Channels;

    public class GenerateStableDiffusionImageHandler : CommandHandlerBase
    {
        public GenerateStableDiffusionImageHandler()
        {
            StableDiffusionClient = new AutomaticStableDiffusionClient();
            ModerationHandler = new ModerationHandler("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");
        }

        protected static ImageQueuer ImageQueuer { get; } = new ImageQueuer();

        public override string CommandName => "generate_image";

        protected virtual AutomaticStableDiffusionClient StableDiffusionClient { get; }

        protected virtual ModerationHandler ModerationHandler { get; }

        public async Task<string> UpscaleAsync(string prompt, string style, long seed, SocketUserMessage m, ISocketMessageChannel imageChannel, string mention = null)
        {
            var images = await ImageQueuer.GenerateImage("(masterpiece, detailed, best quality), " + prompt, style, Guid.NewGuid(), seed, true);

            var files = new List<FileAttachment>();
            var ms = new List<MemoryStream>();

            try
            {
                if (images != null && images.Any())
                {
                    foreach (var image in images)
                    {
                        var stream = new MemoryStream(image.Data);
                        ms.Add(stream);
                        files.Add(new FileAttachment(stream, $"{image.Seed}.png"));
                    }
                    var row1 = new ActionRowBuilder()
                        //.WithButton("V1", style: ButtonStyle.Secondary, customId: "SD_VARIATIONS_V1")
                        .WithButton("🔄", style: ButtonStyle.Secondary, customId: "SD_IMAGE_REGENERATE");

                    var components = new ComponentBuilder()
                        .WithRows(new ActionRowBuilder[] { row1 })
                        .Build();

                    var embed = new EmbedBuilder()
                        .AddField("Prompt", prompt, false)
                        .AddField("Style", style, false)
                        .Build();

                    var fileMessage = await imageChannel.SendFilesAsync(files, text: $"{mention ?? m.Author.Mention} Here's your upscaled image  :arrow_up:  :boom:", components: components, embed: embed);
                }
                else
                {
                    await m.Channel.SendMessageAsync($"Sorry, it looks like something went wrong");
                }
            }
            finally
            {
                foreach (var me in ms)
                {
                    me.Dispose();
                }
            }

            return null;
        }

        public override async Task<string> ExecuteAsync(Dictionary<string, object> args, SocketUserMessage m, ISocketMessageChannel imageChannel, string mention = null)
        {
            var prompt = args["prompt"].ToString();
            var style = args.ContainsKey("style") ? args["style"].ToString() : "mixed";

            if (!(await ModerationHandler.IsAppropriateAsync(prompt)))
            {
                throw new InappropriateRequestException();
            }

            var images = await ImageQueuer.GenerateImage("(masterpiece, detailed, best quality)" + prompt, style, Guid.NewGuid());

            var files = new List<FileAttachment>();
            var ms = new List<MemoryStream>();

            try
            {
                if (images != null && images.Any())
                {
                    foreach (var image in images)
                    {
                        var stream = new MemoryStream(image.Data);
                        ms.Add(stream);
                        files.Add(new FileAttachment(stream, $"{image.Seed}.png"));
                    }

                    var row1 = new ActionRowBuilder()
                        .WithButton("U1", style: ButtonStyle.Secondary, customId: "SD_UPSCALE_U1")
                        .WithButton("U2", style: ButtonStyle.Secondary, customId: "SD_UPSCALE_U2")
                        .WithButton("U3", style: ButtonStyle.Secondary, customId: "SD_UPSCALE_U3")
                        .WithButton("U4", style: ButtonStyle.Secondary, customId: "SD_UPSCALE_U4")
                        .WithButton("🔄", style: ButtonStyle.Secondary, customId: "SD_IMAGE_REGENERATE");

                    //var row2 = new ActionRowBuilder()
                        //.WithButton("V1", style: ButtonStyle.Secondary, customId: "SD_VARIATIONS_V1")
                        //.WithButton("V2", style: ButtonStyle.Secondary, customId: "SD_VARIATIONS_V2")
                        //.WithButton("V3", style: ButtonStyle.Secondary, customId: "SD_VARIATIONS_V3")
                        //.WithButton("V4", style: ButtonStyle.Secondary, customId: "SD_VARIATIONS_V4");

                    var components = new ComponentBuilder()
                        .WithRows(new ActionRowBuilder[] { row1 })
                        .Build();

                    var embed = new EmbedBuilder()
                        .AddField("Prompt", prompt, false)
                        .AddField("Style", style, false)
                    .Build();

                    var fileMessage = await imageChannel.SendFilesAsync(files, text: $"{mention ?? m.Author.Mention} All done! Here's your images  :frame_photo:  :tada:", components: components, embed: embed);
                }
                else
                {
                    await m.Channel.SendMessageAsync($"Sorry, it looks like something went wrong");
                }
            }
            finally
            {
                foreach (var me in ms)
                {
                    me.Dispose();
                }
            }

            return null;
        }
    }
}