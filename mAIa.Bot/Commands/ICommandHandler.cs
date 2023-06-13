using Discord.WebSocket;

namespace DiscordTest.Commands
{
    public interface ICommandHandler
    {
        string CommandName { get; }

        Task<string> ExecuteAsync(Dictionary<string, object> args, SocketUserMessage message, ISocketMessageChannel imageChannel, string userMention = null);
    }
}