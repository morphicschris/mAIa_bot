namespace mAIa.Bot.Chat
{
    using AI.Dev.OpenAI.GPT;
    using Azure;
    using Discord;
    using Discord.WebSocket;
    using DiscordTest;
    using DiscordTest.Commands.Handlers;
    using mAIa.Bot.Services;
    using mAIa.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    public class IndependenceHelper
    {
        private Timer _timer;
        private DateTime _lastKeepAliveCall = DateTime.MinValue;

        protected virtual IChatService ChatService { get; }

        protected virtual IConfiguration Configuration { get; }

        protected virtual SocketChannel Channel { get; }

        protected virtual bool FirstRun { get; set; }

        public IndependenceHelper(
            IConfiguration configuration,
            IChatService chatService,
            SocketChannel channel)
        {
            Configuration = configuration;
            ChatService = chatService;
            Channel = channel;
            FirstRun = true;
        }

        public void StartKeepAlive()
        {
            _timer = new Timer(CheckAndSendKeepAlive, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        }

        private void CheckAndSendKeepAlive(object state)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<mAIaDataContext>();
            optionsBuilder.UseSqlServer(connectionString);
            var messageContext = new mAIaDataContext(optionsBuilder.Options);
            var cutoff = DateTime.Now.AddDays(-2);

            DateTime now = DateTime.Now;
            var previousMessages = messageContext.Messages.Where(e => e.ChannelID == Channel.Id).Where(e => e.Timestamp >= cutoff);

            DateTime lastMessageTime = previousMessages.Count() > 0 ? previousMessages.Max(m => m.Timestamp) : DateTime.MinValue;

            if ((now - _lastKeepAliveCall).TotalHours >= 2 && (now - lastMessageTime).TotalMinutes >= 30)
            {
                if (!FirstRun)
                {
                    SendKeepAliveWithRandomParameter(messageContext);
                }

                _lastKeepAliveCall = now;
                FirstRun = false;
            }

            TimeSpan nextInterval = CalculateNextInterval();

            ConsoleWriter.WriteLine("Next independent message: " + nextInterval + " hours", LogLevel.Info, ConsoleColor.Blue);

            _timer.Change(nextInterval, TimeSpan.FromMilliseconds(-1));
        }

        private void SendKeepAliveWithRandomParameter(mAIaDataContext messageContext)
        {
            Random random = new Random();
            int randomNumber = random.Next(1, 101);
            int parameter = randomNumber <= 65 ? 1 : randomNumber <= 99 ? 2 : 3;
            
            var response = SendKeepAlive(parameter).Result;

            ulong selectedUserId = GetRandomWeightedUserId(messageContext);
            response = response.Replace("{{user}}", $"<@{selectedUserId}>");

            (Channel as ITextChannel).SendMessageAsync(response);
        }

        private TimeSpan CalculateNextInterval()
        {
            Random random = new Random();
            double minIntervalHours = 18.0;
            double maxIntervalHours = 36.0;
            double nextIntervalHours;

            if (_lastKeepAliveCall != DateTime.MinValue)
            {
                TimeSpan timeSinceLastCall = DateTime.UtcNow - _lastKeepAliveCall;
                double remainingTime = Math.Max(minIntervalHours, 4.0 - timeSinceLastCall.TotalHours);
                nextIntervalHours = remainingTime + random.NextDouble() * (maxIntervalHours - minIntervalHours);
            }
            else
            {
                nextIntervalHours = minIntervalHours + random.NextDouble() * (maxIntervalHours - minIntervalHours);
            }

            var intervalTime = DateTime.Now.AddHours(nextIntervalHours).TimeOfDay;

            if (intervalTime >= new TimeSpan(23, 30, 00) || intervalTime <= new TimeSpan(8, 0, 0))
            {
                nextIntervalHours += 8.5D;
            }

            return TimeSpan.FromHours(nextIntervalHours);
        }

        private ulong GetRandomWeightedUserId(mAIaDataContext messageContext)
        {
            DateTime twoDaysAgo = DateTime.Now.AddDays(-2);
            var messagesInLastTwoDays = messageContext.Messages
                .Where(m => m.Timestamp > twoDaysAgo)
                .Where(m => m.DiscordUserID.HasValue)
                .Where(e => e.ChannelID == Channel.Id);
            var userWeights = messagesInLastTwoDays.GroupBy(m => m.DiscordUserID)
                                                   .Select(g => new { UserId = g.Key, Weight = g.Count() })
                                                   .ToList();

            int totalWeight = userWeights.Sum(w => w.Weight);
            int randomWeight = new Random().Next(1, totalWeight + 1);
            ulong selectedUserId = 0;

            foreach (var userWeight in userWeights)
            {
                if (randomWeight <= userWeight.Weight)
                {
                    selectedUserId = userWeight.UserId.Value;
                    break;
                }

                randomWeight -= userWeight.Weight;
            }

            return selectedUserId;
        }

        private async Task<string> SendKeepAlive(int parameter)
        {
            return await ChatService.IndependenceQueryAsync($"INDEPENDENT_ACTION [{DateTime.Now.DayOfWeek} {DateTime.Now:HH:mm}] 1", parameter, Channel);
        }
    }
}
