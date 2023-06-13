namespace mAIa.Bot.Commands.DiscordSlashCommands
{
    using Discord;
    using Discord.WebSocket;
    using DiscordTest;

    public class SetupCommand : ISlashCommand
    {
        public string CommandName => "maia_setup";

        public SlashCommandBuilder GetCommandBuilder()
        {
            return new SlashCommandBuilder()
                .WithName(CommandName)
                .WithDescription("Sets mAIa up with the channels and settings she needs");
        }

        public async Task HandleAsync(DiscordSocketClient client, SocketSlashCommand slashCommand)
        {
            try
            {
                if (!slashCommand.GuildId.HasValue)
                {
                    return;
                }

                var server = client.GetGuild(slashCommand.GuildId.Value);

                if (!server.GetUser(slashCommand.User.Id).GuildPermissions.Administrator)
                {
                    await slashCommand.RespondAsync("You must be a server admin to run this command", ephemeral: true);

                    return;
                }

                await slashCommand.RespondAsync("Setting up channels and emotes...", ephemeral: true);
                
                await CreateEmote(server, "maia_smile");
                await CreateEmote(server, "maia_grin");
                await CreateEmote(server, "maia_blush");
                await CreateEmote(server, "maia_heart");
                await CreateEmote(server, "maia_angry");
                await CreateEmote(server, "maia_sleep");

                var textChannelExists = server.TextChannels.Any(e => e.Name == "maia");

                var category = await server.CreateCategoryChannelAsync("mAIa");

                if (!textChannelExists)
                {
                    ConsoleWriter.WriteLine("Creating maia text channel", LogLevel.Info, ConsoleColor.Yellow);

                    var channel = await server.CreateTextChannelAsync("maia-chat", e =>
                    {
                        e.CategoryId = category.Id;
                        e.Topic = "Use this channel to talk to mAIa without having to @ or reply to her. See pinned for details.";
                    });

                    var welcomeImage = await channel.SendFileAsync(@"Assets\welcome.png", string.Empty);

                    var detailsMessage = await channel.SendMessageAsync("\nThis is the free chat channel for talking with me. " +
                        "You can type in here, and I'll automatically reply to you without having to mention me by name or reply to my posts. " +
                        "If you want to talk to another user in here without me responding, you can @ that user. If you want to talk to me " +
                        "about another user, @ us both.\n\n" +
                        "Here are some of the things I can help with:\n" +
                        "- **Answering questions** - you can ask me about problems you're having or if you want to know how to do something.\n" +
                        "- **General chat** - I can talk about anything you'd like, as long as it's appropriate. I'm eager to learn about new things, and " +
                        "I'll start to learn more about you as we talk.\n\n" +
                        "I look forward to talking with you!");

                    await detailsMessage.PinAsync();
                    await welcomeImage.PinAsync();
                }

                //var iamgeChannelExists = server.TextChannels.Any(e => e.Name == "maia");

                //if (!iamgeChannelExists)
                //{
                //    var imageChannel = await server.CreateTextChannelAsync("maia-images", e =>
                //    {
                //        e.CategoryId = category.Id;
                //        e.Topic = "When you ask mAIa to generate an image, she'll post it here.";
                //    });

                //    var paintingMessage = await imageChannel.SendFileAsync(@"Assets\painting.png", string.Empty);
                //    await paintingMessage.PinAsync();

                //    var everyonePermissions = new OverwritePermissions(sendMessages: PermValue.Deny);
                //    await imageChannel.AddPermissionOverwriteAsync(server.EveryoneRole, everyonePermissions);

                //    var rolePermissions = new OverwritePermissions(sendMessages: PermValue.Allow);
                //    await imageChannel.AddPermissionOverwriteAsync(server.Roles.First(e => e.Name.ToLower() == "maia"), rolePermissions);
                //}
            }
            catch (Exception)
            {
                await slashCommand.Channel.SendMessageAsync("Sorry, something went wrong during setup");
            }
        }

        private static async Task CreateEmote(SocketGuild server, string emoteName)
        {
            if (!server.Emotes.Any(e => e.Name == emoteName))
            {
                await server.CreateEmoteAsync(emoteName, new Image($@"Assets\Emotes\{emoteName.Replace("_", "-")}.png"));
            }
        }
    }
}