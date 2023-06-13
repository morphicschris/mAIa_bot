using Discord.WebSocket;
using System.Text;

namespace DiscordTest.Commands
{
    public abstract class CommandHandlerBase : ICommandHandler
    {
        public abstract string CommandName { get; }

        public abstract Task<string> ExecuteAsync(Dictionary<string, object> args, SocketUserMessage message, ISocketMessageChannel imageChannel, string userMention = null);

        public static string DictionaryToString(Dictionary<string, object> dictionary)
        {
            StringBuilder sb = new StringBuilder();

            if (dictionary != null)
            {
                foreach (KeyValuePair<string, object> kvp in dictionary)
                {
                    sb.AppendLine($"{kvp.Key}: {kvp.Value}");
                }
            }

            return sb.ToString();
        }
    }
}