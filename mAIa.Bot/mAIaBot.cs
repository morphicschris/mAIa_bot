namespace mAIa.Bot
{
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using DiscordTest;
    using DiscordTest.Commands;
    using DiscordTest.Commands.Handlers;
    using DiscordTest.Moderation;
    using mAIa.Bot.Chat;
    using mAIa.Bot.Commands.DiscordSlashCommands;
    using mAIa.Bot.Commands.SlashCommands;
    using mAIa.Bot.Moderation;
    using mAIa.Bot.Services;
    using mAIa.Data;
    using mAIa.Data.Model;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;

    public class mAIaBot : IHostedService
    {
        public mAIaBot(
            IConfiguration configuration,
            IChatService chatService)
        {
            Configuration = configuration;
            ChatService = chatService;

            Commands = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.GetInterface(nameof(ICommandHandler)) != null);

            DiscordBotToken = configuration.GetSection("AppSettings:DiscordBotToken").Value.ToString();

            var apiKey = configuration.GetSection("AppSettings:OpenApiKey").Value;
            ModerationHandler = new ModerationHandler(apiKey);

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<mAIaDataContext>();
            optionsBuilder.UseSqlServer(connectionString);

            DbOptions = optionsBuilder;
        }

        protected virtual IConfiguration Configuration { get; }

        protected virtual string DiscordBotToken { get; }

        protected virtual IEnumerable<Type> Commands { get; }

        protected virtual IChatService ChatService { get; }

        protected virtual ModerationHandler ModerationHandler { get; }

        protected virtual DbContextOptionsBuilder<mAIaDataContext> DbOptions { get; }

        protected virtual IndependenceHelper IndependenceHelper { get; set; }

        protected virtual DiscordSocketClient DiscordClient { get; set; }

        public async Task InitializeAsync()
        {
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            DiscordClient = new DiscordSocketClient(config);

            var commandService = new CommandService();

            var slashCommands = new List<ISlashCommand>
            {
                new SetupCommand(),
                new ListTraitsCommand(Configuration)
            };

            var slashCommandModule = new SlashCommandModule(DiscordClient, commandService, slashCommands);
            await slashCommandModule.InitializeAsync();

            DiscordClient.ButtonExecuted += ButtonComponentHandler;

            DiscordClient.Log += LogAsync;
            DiscordClient.Ready += ReadyAsync;
            DiscordClient.MessageReceived += HandleCommandAsync;

            await DiscordClient.LoginAsync(TokenType.Bot, DiscordBotToken);
            await DiscordClient.StartAsync();

            await Task.Delay(-1);
        }

        public async Task ButtonComponentHandler(SocketMessageComponent component)
        {
            Task.Run(async () =>
            {
                var embed = component.Message.Embeds.First();
                var dictionary = new Dictionary<string, object>();
                var prompt = embed.Fields.First(e => e.Name == "Prompt").Value;
                var style = embed.Fields.First(e => e.Name == "Style").Value;
                dictionary.Add("prompt", prompt);
                dictionary.Add("style", style);
                var sdImageCommand = new GenerateStableDiffusionImageHandler();

                ISocketMessageChannel channel = component.Channel;

                if (component.GuildId.HasValue)
                {
                    var serverId = component.GuildId.Value;
                    var server = DiscordClient.GetGuild(serverId);
                    var imageChannel = server.TextChannels.FirstOrDefault(e => e.Name.ToLower() == "maia-images");

                    channel = imageChannel ?? component.Channel;
                }

                switch (component.Data.CustomId)
                {
                    case "SD_IMAGE_REGENERATE":
                        {
                            await component.RespondAsync($"Generating some similar images...", ephemeral: true);
                            await sdImageCommand.ExecuteAsync(dictionary, component.Message, channel, component.User.Mention);
                            break;
                        }
                    case "SD_UPSCALE_U1":
                    case "SD_UPSCALE_U2":
                    case "SD_UPSCALE_U3":
                    case "SD_UPSCALE_U4":
                        {
                            var imageNumber = Convert.ToInt32(component.Data.CustomId.Substring(component.Data.CustomId.Length - 1));
                            var filename = component.Message.Attachments.Skip(imageNumber - 1).First().Filename;
                            var seed = Convert.ToInt64(filename.Replace(".png", string.Empty));
                            await component.RespondAsync($"Upscaling image {imageNumber}...", ephemeral: true);
                            await sdImageCommand.UpscaleAsync(prompt, style, seed, component.Message, channel, component.User.Mention);
                            break;
                        }
                }
            });
        }

        private async Task LogAsync(LogMessage logMessage)
        {
            ConsoleWriter.WriteLine(logMessage.ToString(), LogLevel.Debug, ConsoleColor.DarkGray);
        }

        private async Task ReadyAsync()
        {
            ConsoleWriter.WriteLine("mAIa is now connected!", LogLevel.Info, ConsoleColor.Green);

            var channel = DiscordClient.GetChannel(1099444521651163166);
            IndependenceHelper = new IndependenceHelper(Configuration, ChatService, channel);
        }

        private Task DiscordAudioClient_SpeakingUpdated(ulong discordUserId, bool speaking)
        {
            throw new NotImplementedException();
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            var MessageContext = new mAIaDataContext(DbOptions.Options);
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            Task.Run(async () =>
            {
                try
                {
                    if ((message.Channel is IDMChannel && !message.Author.IsBot) || (message.Channel.Name.ToLower() == "maia-chat" && message.Author.Id != 1097784208228884481 && !(message.MentionedUsers.Any() && !message.MentionedUsers.Any(e => e.Id == 1097784208228884481))) || message.MentionedUsers.Any(e => e.Id == DiscordClient.CurrentUser.Id) || message.Content.Contains("@mAIa") || message.Content.Contains("<@1097784208228884481>"))
                    {
                        using (message.Channel.EnterTypingState())
                        {
                            var dbUser = await MessageContext.Users.FindAsync(message.Author.Id);
                        
                            if (dbUser == null)
                            {
                                MessageContext.Users.Add(new User
                                {
                                    DiscordUserID = message.Author.Id,
                                    DiscordUsername = message.Author.Username
                                });

                                await MessageContext.SaveChangesAsync();

                                dbUser = await MessageContext.Users.FindAsync(message.Author.Id);
                            }

                            if (dbUser.InnappropriateWarningIgnoredFrom.HasValue)
                            {
                                return;
                            }

                            if (!await ModerationHandler.IsAppropriateAsync(message.Content))
                            {
                                throw new InappropriateRequestException();
                            }

                            ChatQueryResponse response = null;

                            var content = message.Content;

                            if (message.MentionedUsers.Any())
                            {
                                var mentionedUsers = message.MentionedUsers.Select(e => new
                                {
                                    e.Id,
                                    e.Username
                                });

                                foreach (var user in mentionedUsers)
                                {
                                    content = content
                                        .Replace($"<@{user.Id}>", user.Username)
                                        .Replace($"@{user.Username}", user.Username);
                                }
                            }

                            response = await ChatService.QueryAsync(content, message.Author, message.Channel);

                            try
                            {
                                if (response != null)
                                {
                                    var result = await HandleChatMessage(response, message);
                                }
                            }
                            catch (Exception ex)
                            {
                                ConsoleWriter.WriteError(ex.Message, LogLevel.Error, response);
                            }
                        }
                    }
                }
                catch (InappropriateRequestException)
                {
                    ConsoleWriter.WriteError(message.Content, LogLevel.Warning);

                    var dbUser = await MessageContext.Users.FindAsync(message.Author.Id);

                    if (dbUser != null)
                    {
                        ++dbUser.InappropriateWarningCount;
                        await MessageContext.SaveChangesAsync();

                        if (dbUser.InappropriateWarningCount >= 3)
                        {
                            dbUser.InnappropriateWarningIgnoredFrom = DateTime.Now;
                            await MessageContext.SaveChangesAsync();

                            await message.ReplyAsync("You've been warned too many times about sending inappropriate content. I will now ignore any requests you make.", allowedMentions: new AllowedMentions());
                        }
                        else
                        {
                            await message.ReplyAsync(
                                "It looks like you're trying to get me to do or talk about something inappropriate. " +
                                "This is your " + (dbUser.InappropriateWarningCount == 1 ? "1st" : "2nd and final") + " warning. After 3 warnings I will refuse to talk to you anymore. " +
                                "Please be considerate.",
                                allowedMentions: new AllowedMentions());
                        }
                    }
                }
                catch (Exception ex)
                {
                    ConsoleWriter.WriteError(ex.Message, LogLevel.Error);
                }
            });
        }

        public async Task<string> ExecuteCommandAsync(string commandName, Dictionary<string, object> args, SocketUserMessage message)
        {
            try
            {
                var commandHandlerType = Commands.FirstOrDefault(t =>
                    ((ICommandHandler)Activator.CreateInstance(t)).CommandName == commandName);

                if (commandHandlerType != null)
                {
                    var c = message.Channel as SocketGuildChannel;

                    var server =  c?.Guild?.Id == null ? null :DiscordClient.GetGuild(c.Guild.Id);
                    var imageChannel = server?.TextChannels.FirstOrDefault(e => e.Name.ToLower() == "maia-images") ?? message.Channel;

                    var commandHandler = (ICommandHandler)Activator.CreateInstance(commandHandlerType);
                    return await commandHandler.ExecuteAsync(args, message, imageChannel);
                }
                else
                {
                    ConsoleWriter.WriteLine($"Unknown command: {commandName}", LogLevel.Error, ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                ConsoleWriter.WriteLine($"Error executing command: {ex.Message}", LogLevel.Error, ConsoleColor.Red);
                return null;
            }
        }

        public async Task<ChatQueryResponse> HandleChatMessage(
            ChatQueryResponse message,
            SocketUserMessage m)
        {
            ConsoleWriter.WriteLine(message, LogLevel.Info);

            try
            {
                if (!string.IsNullOrEmpty(message.Thoughts.Speak))
                {
                    if (m.Channel is IDMChannel)
                    {
                        await m.Channel.SendMessageAsync(message.Thoughts.Speak);
                    }
                    else
                    {
                        await m.ReplyAsync(message.Thoughts.Speak, allowedMentions: new AllowedMentions());
                    }
                }

                if (message.Command != null && !string.IsNullOrEmpty(message.Command.Name) && message.Command.Name != "do_nothing")
                {
                    ConsoleWriter.WriteLine($"Executing command {message.Command.Name} with args " + CommandHandlerBase.DictionaryToString(message.Command.Args), LogLevel.Info, ConsoleColor.Yellow);

                    var commandResult = await ExecuteCommandAsync(message.Command.Name, message.Command.Args, m);

                    if (!string.IsNullOrEmpty(commandResult))
                    {
                        message = await ChatService.QueryAsync(commandResult, m.Author, m.Channel, ChatMessageType.System);

                        HandleChatMessage(message, m);
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleWriter.WriteError(ex.Message, LogLevel.Error);
            }

            return message;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await InitializeAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }

        protected Stream CreateStream(string path)
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-y -hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 \"{path.Replace("mp3", "opus")}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });

            p.WaitForExit();

            return File.OpenRead(path.Replace("mp3", "opus"));
        }
    }
}