namespace mAIa.Bot.Commands.SlashCommands
{
    using Discord;
    using Discord.WebSocket;
    using mAIa.Bot.Commands.DiscordSlashCommands;
    using mAIa.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    internal class ListTraitsCommand : ISlashCommand
    {
        public ListTraitsCommand(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public string CommandName => "maia_list_traits";

        public virtual string ConnectionString { get; }

        public SlashCommandBuilder GetCommandBuilder()
        {
            return new SlashCommandBuilder()
                .WithName(CommandName)
                .WithDescription("Gets a list of information which mAIa has remembered about you");
        }

        public async Task HandleAsync(DiscordSocketClient client, SocketSlashCommand slashCommand)
        {
            await slashCommand.RespondAsync("Getting user traits...", ephemeral: true);

            var optionsBuilder = new DbContextOptionsBuilder<mAIaDataContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            var messageContext = new mAIaDataContext(optionsBuilder.Options);

            var discordUserId = slashCommand.User.Id;

            var traits = messageContext.UserTraits
                .Where(e => e.DiscordUserID == discordUserId)
                .OrderBy(e => e.DateAdded);

            var traitsList = "* None";
            var dmChannel = await slashCommand.User.CreateDMChannelAsync();

            if (traits.Any())
            {
                int skipped = 0;

                while (traits.Skip(skipped).Any())
                {
                    traitsList = string.Join("\n", traits.Skip(skipped).Take(10).Select(e => $"**[{e.Key}]** {e.Value}"));

                    await dmChannel.SendMessageAsync((skipped == 0 ? "Traits:\n------------------\n" : string.Empty) + $"\n{traitsList}");

                    skipped += 10;
                }
            }
        }
    }
}