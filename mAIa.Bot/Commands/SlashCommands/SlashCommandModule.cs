namespace mAIa.Bot.Commands.SlashCommands
{
    using Discord.Commands;
    using Discord.WebSocket;
    using mAIa.Bot.Commands.DiscordSlashCommands;

    public class SlashCommandModule
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly List<ISlashCommand> _slashCommands;

        public SlashCommandModule(DiscordSocketClient client, CommandService commandService, IEnumerable<ISlashCommand> slashCommands)
        {
            _client = client;
            _commandService = commandService;
            _slashCommands = slashCommands.ToList();
        }

        public async Task InitializeAsync()
        {
            _client.Ready += RegisterSlashCommandsAsync;
            _client.InteractionCreated += HandleInteractionCreatedAsync;
        }

        private async Task RegisterSlashCommandsAsync()
        {
            foreach (var command in _slashCommands)
            {
                await _client.Rest.CreateGlobalCommand(command.GetCommandBuilder().Build());
            }
        }

        private async Task HandleInteractionCreatedAsync(SocketInteraction interaction)
        {
            if (interaction is SocketSlashCommand slashCommand)
            {
                var command = _slashCommands.FirstOrDefault(c => c.CommandName == slashCommand.CommandName);
                if (command != null)
                {
                    await command.HandleAsync(_client, slashCommand);
                }
            }
        }
    }
}