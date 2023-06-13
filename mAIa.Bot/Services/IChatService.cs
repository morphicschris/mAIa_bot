namespace mAIa.Bot.Services
{
    using Discord.WebSocket;
    using mAIa.Bot.Chat;

    public interface IChatService
    {
        Task<ChatQueryResponse> QueryAsync(
            string message,
            SocketUser author,
            ISocketMessageChannel channel,
            ChatMessageType messageType = ChatMessageType.User,
            int retries = 3);

        Task<string> IndependenceQueryAsync(string message, int parameter, SocketChannel channel);
    }
}