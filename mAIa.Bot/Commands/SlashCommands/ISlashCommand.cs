namespace mAIa.Bot.Commands.DiscordSlashCommands
{
    using Discord;
    using Discord.WebSocket;

    public interface ISlashCommand
    {
        string CommandName { get; }

        SlashCommandBuilder GetCommandBuilder();
        
        Task HandleAsync(
            DiscordSocketClient client,
            SocketSlashCommand slashCommand);
    }
}