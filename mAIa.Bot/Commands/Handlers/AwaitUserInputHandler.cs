using Discord.WebSocket;

namespace DiscordTest.Commands.Handlers
{
    public  class AwaitUserInputHandler : CommandHandlerBase
    {
        public override string CommandName => "await_input";

        public override async Task<string> ExecuteAsync(Dictionary<string, object> args, SocketUserMessage message, ISocketMessageChannel imageChannel, string userMention = null)
        {
            return string.Empty;
        }
    }
}