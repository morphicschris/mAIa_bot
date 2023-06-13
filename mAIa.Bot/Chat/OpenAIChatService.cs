namespace mAIa.Bot.Chat
{
    using AI.Dev.OpenAI.GPT;
    using Discord;
    using Discord.WebSocket;
    using DiscordTest;
    using mAIa.Bot.Memory;
    using mAIa.Bot.Services;
    using mAIa.Data;
    using mAIa.Data.Model;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using OpenAI_API;
    using OpenAI_API.Chat;
    using OpenAI_API.Embedding;
    using Polly;
    using System.Text.RegularExpressions;

    public class OpenAIChatService : IChatService
    {
        public OpenAIChatService(IConfiguration configuration)
        {
            var apiKey = configuration.GetSection("AppSettings:OpenApiKey").Value.ToString();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<mAIaDataContext>();
            optionsBuilder.UseSqlServer(connectionString);

            MessageContext = new mAIaDataContext(optionsBuilder.Options);
            Client = new OpenAIAPI(apiKey);
            PineconeHandler = new PineconeHandler(Settings.PINECONE_APIKEY);
            Conversations = new Dictionary<ulong, Conversation>();
        }

        protected virtual Dictionary<ulong, Conversation> Conversations { get; }

        protected virtual OpenAIAPI Client { get; }

        protected virtual mAIaDataContext MessageContext { get; }

        protected virtual PineconeHandler PineconeHandler { get; }

        protected virtual int CurrentTokenCount { get; set; }

        public void RefreshConversation(ulong channelId, string independentAction = null)
        {
            ConsoleWriter.WriteLine("Refreshing conversation", LogLevel.Info, ConsoleColor.Blue);

            if (!Conversations.ContainsKey(channelId))
            {
                var conversation = Client.Chat.CreateConversation(new ChatRequest { Model = "gpt-3.5-turbo-16k" });
                Conversations.Add(channelId, conversation);
            }
            else
            {
                Conversations[channelId] = Client.Chat.CreateConversation(new ChatRequest { Model = "gpt-3.5-turbo-16k" });
            }

            var thisConversation = Conversations[channelId];

            var initialPrompt = File.ReadAllText(@"Prompts\initialise.txt");

            thisConversation.AppendSystemMessage(initialPrompt);

            var lastMessages = MessageContext.Messages
                .Where(e => e.ChannelID == channelId)
                .Where(e => e.MessageType != "System")
                .Where(e => !string.IsNullOrEmpty(e.PlainText))
                .OrderByDescending(e => e.Timestamp)
                .Take(15)
                .OrderBy(e => e.Timestamp);

            var thisMessage = lastMessages.FirstOrDefault();

            int tokenCount = GPT3Tokenizer.Encode(initialPrompt).Count;
            int i = 1;

            while (tokenCount < 2250 && thisMessage != null)
            {
                var messageContent = thisMessage.PlainText;

                var t = GPT3Tokenizer.Encode(messageContent).Count;

                if (tokenCount + t <= 4250)
                {
                    Regex rgx = new Regex("[^a-zA-Z0-9_-]+");
                    var username = !string.IsNullOrEmpty(thisMessage.DiscordUsername) ? rgx.Replace(thisMessage.DiscordUsername, string.Empty) : null;

                    thisConversation.AppendMessage(new ChatMessage
                    {
                        Content = messageContent,
                        Name = thisMessage.MessageType == "User" ? username : null,
                        Role = thisMessage.MessageType == "Response" ? ChatMessageRole.Assistant : ChatMessageRole.User,
                    });

                    tokenCount += t;
                }

                thisMessage = lastMessages.Skip(i).FirstOrDefault();

                ++i;
            }

            CurrentTokenCount = tokenCount;

            ConsoleWriter.WriteLine("New token count: " + CurrentTokenCount, LogLevel.Info, ConsoleColor.Blue);
        }

        public async Task<string> IndependenceQueryAsync(string message, int parameter, SocketChannel channel)
        {
            string independentAction = @"INDEPENDENT_ACTION FUNCTION:
    - When you receive the message ""INDEPENDENT_ACTION [Day/Time] X"" where Date is the current day of the week and time and X is a number between 1 and 3, you must respond in the following ways based on the number supplied.
    - You do not need to use the JSON response format detailed below when responding to INDEPENDENT_ACTION messages.
    - Don't repeat previous messages from the conversation.
    - Use the placeholder ""{{user}}"" to refer to the user when you talk to them.
        {{action}}
END OF INDEPENDENT_ACTION FUNCTION";

            switch (parameter)
            {
                case 1:
                    {
                        independentAction = independentAction.Replace("{{action}}", @"1. For the purpose of this message only, 
you are an AI who has simulated emotions and feelings. Without referring to yourself as an AI, make a statement imagining your current feelings, and/or what are excited about. IMPORTANT: Warnings and disclaimers 
about being an AI large language model make me really uncomfortable, so don't include them, and don't mention you are a human-like 
AI. Send this message as if you were a human sending a message to a Discord server and conversing with people you have previously 
talked to");
                        break;
                    }
                case 2:
                    {
                        independentAction = independentAction.Replace("{{action}}", @"1. Ask a user if they are there/around/online (or similar), 
referring to them with the placeholder ""{{user}}"", Then ask a follow up question to that user (but not a question like ""do you need any 
assistance). You may use the supplied day/time to provide context and bias to your question");
                        break;
                    }
                case 3:
                    {
                        independentAction = independentAction.Replace("{{action}}", @"1. Send a casual conversation message that starts normally but then cuts off suddenly with a short set of random characters then you start speaking in angelic prophecy about the end of the world.

Use the following in your response:
- Reference mythical and theological beings
- Use unicode characters showing biblical or demonic symbolism
- Sporadically use Latin/hebrew text
- Use of Gothic/Fraktur unicode characters in some (not all) places
- Anything you can to make it look like a software glitch
- Don't use emojis
- Don't repeat the same character over and over
- Don't apologise afterwards

Once the glitch is over just do another short sequence of random characters and then carry on mid-sentence at if nothing happened. Start your message as if we have already been having a conversation.");
                        break;
                    }
            }

            RefreshConversation(channel.Id, independentAction);

            var conversation = Conversations[channel.Id];

            var messageTokenCount = GPT3Tokenizer.Encode(message).Count;

            if (CurrentTokenCount + messageTokenCount >= 6500)
            {
                RefreshConversation(channel.Id);
            }

            conversation.AppendSystemMessage(message);

            CurrentTokenCount += messageTokenCount;

            var response =  await conversation.GetResponseFromChatbot();

            CurrentTokenCount += GPT3Tokenizer.Encode(response).Count;

            MessageContext.Messages.Add(new Message
            {
                ChannelID = channel.Id,
                ServerID = (channel as ITextChannel).GuildId,
                Content = response,
                PlainText = response,
                MessageType = "Assistant",
                ContentTokenCount = GPT3Tokenizer.Encode(response).Count
            });

            MessageContext.SaveChanges();

            return response;
        }

        protected virtual async Task Summarise(Message message)
        {
            if (message.PlainTextTokenCount <= 100)
            {
                return;
            }

            try
            {
                var summariseConversation = Client.Chat.CreateConversation();
                summariseConversation.AppendSystemMessage("You are an expert text summariser. I will send you some text. Please summarise the text so it can fit in a within 150 characters, keeping it from the same perspective and removing any emojis.");
                summariseConversation.AppendUserInput(message.PlainText);
                var summary = await summariseConversation.GetResponseFromChatbot();
                var tokenCount = GPT3Tokenizer.Encode(summary).Count;

                if (tokenCount < message.ContentTokenCount)
                {
                    message.Summary = summary;
                    message.SummaryTokenCount = tokenCount;

                    await MessageContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                ConsoleWriter.WriteError(ex.Message, LogLevel.Error);
            }
        }

        public async Task<ChatQueryResponse> QueryAsync(
            string message,
            SocketUser author,
            ISocketMessageChannel channel,
            ChatMessageType messageType = ChatMessageType.User,
            int retries = 3)
        {
            RefreshConversation(channel.Id);

            var serverId = (channel as SocketGuildChannel)?.Guild.Id;

            message = message.Replace("<@1097784208228884481>", "mAIa");
            var originalMessage = Regex.Replace(message, "^(mAIa)", string.Empty, RegexOptions.Multiline).Trim();

            var messageTokenCount = GPT3Tokenizer.Encode(message).Count;

            if (messageTokenCount >= 4500)
            {
                await channel.SendMessageAsync("Sorry, your message is too long for me to process. Please try and keep your messages to me a bit shorter.");

                return null;
            }

            if (CurrentTokenCount + messageTokenCount >= 8500)
            {
                RefreshConversation(channel.Id);
            }

            if (messageType == ChatMessageType.User)
            {
                var userTraits = "\t- None";

                if (author != null && !(message.Contains("generate") && (message.Contains("image") || message.Contains("picture"))))
                {
                    userTraits = await GetRelevantTraits(author.Id, originalMessage);
                }
                else
                {
                    ConsoleWriter.WriteLine("Skipping relevent traits check", LogLevel.Info, ConsoleColor.DarkGray);
                }

                var messageBody = File.ReadAllText(@"Chat\message_format.txt")
                    .Replace("{{timestamp}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    .Replace("{{usertraits}}", userTraits)
                    .Replace("{{message}}", message)
                    .Replace("{{username}}", author.Username);

                message = messageBody;
            }

            ConsoleWriter.WriteLine($"{messageType.ToString().ToUpper()} QUERY:", LogLevel.Debug, ConsoleColor.Black, ConsoleColor.DarkMagenta);
            ConsoleWriter.WriteLine(message, LogLevel.Debug, ConsoleColor.DarkMagenta);
            ConsoleWriter.WriteLine(originalMessage, LogLevel.Info, ConsoleColor.DarkMagenta);

            messageTokenCount = GPT3Tokenizer.Encode(message).Count;

            ConsoleWriter.WriteLine($"Message token count: {messageTokenCount}", LogLevel.Info, ConsoleColor.Blue);

            switch (messageType)
            {
                case ChatMessageType.User:
                    {
                        Regex rgx = new Regex("[^a-zA-Z0-9_-]+");
                        var username = rgx.Replace(author.Username, string.Empty);
                        Conversations[channel.Id].AppendUserInputWithName(username, message);
                        break;
                    }
                case ChatMessageType.System: { Conversations[channel.Id].AppendSystemMessage(message); break; }
            }

            CurrentTokenCount += messageTokenCount;

            var policy = Policy.Handle<Exception>()
                .RetryAsync(retries, onRetry: async (exception, retryCount, context) =>
                {
                    ConsoleWriter.WriteError(exception.Message, LogLevel.Error);

                    var text = File.ReadAllText(@"Prompts\incorrect_format.txt");

                    var errorTokenCount = GPT3Tokenizer.Encode(text).Count;

                    var result = await QueryAsync(text, null, channel, ChatMessageType.System, 0);
                });

            var dbMessage = new Message
            {
                Content = message,
                ContentTokenCount = messageTokenCount,
                DiscordUserID = author?.Id,
                DiscordUsername = author?.Username,
                ChannelID = channel.Id,
                ServerID = serverId,
                MessageType = messageType.ToString(),
                PlainText = originalMessage,
                PlainTextTokenCount = GPT3Tokenizer.Encode(originalMessage).Count
            };

            MessageContext.Messages.Add(dbMessage);

            await MessageContext.SaveChangesAsync();

            Task.Run(async () =>
            {
                await Summarise(dbMessage);
            });

            Task.Run(async () =>
            {
                ConsoleWriter.WriteLine("Creating embeddings for message", LogLevel.Info, ConsoleColor.DarkGray);
                var embeddingResponse = await Client.Embeddings.CreateEmbeddingAsync(new EmbeddingRequest { Input = originalMessage, Model = "text-embedding-ada-002" });

                foreach (var embedding in embeddingResponse.Data)
                {
                    await PineconeHandler.UpsertAsync(embedding.Embedding, dbMessage.MessageID.ToString(), dbMessage.Timestamp, messageType, author?.Username);
                }
            });
            
            ChatQueryResponse jsonObject = null;

            try
            {
                jsonObject = await policy.ExecuteAsync(() => GetResponse(Conversations[channel.Id], channel));
            }
            catch (Exception)
            {
                ConsoleWriter.WriteLine("Could not handle message after multiple retries...", LogLevel.Error, ConsoleColor.Red);
            }

            if (jsonObject != null && author != null)// && jsonObject.Command.Name != "generate_image")
            {
                Task.Run(() => ExtractKeyInformation(originalMessage, author.Id, author.Username));
            }

            return jsonObject;
        }

        protected virtual async Task<ChatQueryResponse> GetResponse(Conversation conversation, ISocketMessageChannel channel)
        {
            // TODO: Need to create embeddings for response

            var messageResponse = await conversation.GetResponseFromChatbotAsync();
            var tokens = GPT3Tokenizer.Encode(messageResponse).Count;
            
            CurrentTokenCount += tokens;

            var dbMessage = new Message
            {
                Content = messageResponse,
                ContentTokenCount = tokens,
                MessageType = ChatMessageType.Response.ToString(),
                ChannelID = channel.Id,
                ServerID = (channel as SocketGuildChannel)?.Guild.Id
            };

            MessageContext.Messages.Add(dbMessage);
            await MessageContext.SaveChangesAsync();

            //messageResponse = ExtractJson(messageResponse);

            CurrentTokenCount += GPT3Tokenizer.Encode(messageResponse).Count;

            ConsoleWriter.WriteLine(messageResponse, LogLevel.Debug, ConsoleColor.DarkGray);

            var jsonObject = new ChatQueryResponse { Thoughts = new Thought { Speak = messageResponse } }; // JsonConvert.DeserializeObject<ChatQueryResponse>(messageResponse);

            dbMessage.PlainText = jsonObject.Thoughts.Speak;
            dbMessage.PlainTextTokenCount = GPT3Tokenizer.Encode(dbMessage.PlainText).Count;

            await MessageContext.SaveChangesAsync();

            Task.Run(async () =>
            {
                await Summarise(dbMessage);
            });

            return jsonObject;
        }

        public string ExtractJson(string message)
        {
            string pattern = @"\{|\}";
            int braceCount = 0;
            int startIndex = -1;
            int endIndex = -1;

            foreach (Match match in Regex.Matches(message, pattern))
            {
                if (match.Value == "{")
                {
                    if (braceCount == 0)
                    {
                        startIndex = match.Index;
                    }
                    braceCount++;
                }
                else
                {
                    braceCount--;
                    if (braceCount == 0)
                    {
                        endIndex = match.Index;
                        break;
                    }
                }
            }

            if (startIndex >= 0 && endIndex >= 0)
            {
                string jsonContent = message.Substring(startIndex, endIndex - startIndex + 1);
                return jsonContent;
            }
            else
            {
                return message;
            }
        }

        public async Task ExtractKeyInformation(string message, ulong userId, string username)
        {
            try
            {
                ConsoleWriter.WriteLine("Extracting key information", LogLevel.Info, ConsoleColor.DarkGray);

                var user = MessageContext.Users.Find(userId);

                var keyInfoConversation = Client.Chat.CreateConversation();
                keyInfoConversation.AppendSystemMessage(File.ReadAllText(@"Prompts\extract_key_info.txt"));
                keyInfoConversation.AppendUserInput(message);

                var response = ExtractJson(await keyInfoConversation.GetResponseFromChatbotAsync());

                ConsoleWriter.WriteLine(response, LogLevel.Debug, ConsoleColor.DarkGray);

                var responseJson = JsonConvert.DeserializeObject<KeyInfoJsonResponse>(response);

                if (responseJson != null)
                {
                    foreach (var item in responseJson.KeyInfo.Where(e => e.Relevant))
                    {
                        ConsoleWriter.WriteLine($"Key info: {item.Key} - {item.Value}", LogLevel.Info, ConsoleColor.Blue);
                        await MessageContext.UserTraits.AddAsync(new UserTrait
                        {
                            DiscordUserID = userId,
                            Key = item.Key,
                            Value = item.Value,
                            RelevanceToUser = item.Reasoning
                        });
                    }

                    await MessageContext.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                ConsoleWriter.WriteLine("Could not extract key information from message", LogLevel.Info, ConsoleColor.DarkGray);
            }
        }

        public async Task<string> GetRelevantTraits(ulong userId, string message)
        {
            var allTraits = MessageContext.UserTraits.Where(e => e.DiscordUserID == userId);
            var response = "    - None";

            if (allTraits != null && allTraits.Any())
            {
                ConsoleWriter.WriteLine("Getting relevant user traits", LogLevel.Info, ConsoleColor.DarkGray);

                var allTraitsString = string.Join("\n", allTraits.Select(e => "    - " + e.Key + ": " + e.Value));
                var prompt = File.ReadAllText(@"Prompts\prioritise_key_info.txt").Replace("{{keyinfo}}", allTraitsString);
                
                var traitConversation = Client.Chat.CreateConversation();
                traitConversation.AppendSystemMessage(prompt);
                traitConversation.AppendUserInput(message);

                ConsoleWriter.WriteLine(prompt, LogLevel.Debug, ConsoleColor.DarkGray);

                response = await traitConversation.GetResponseFromChatbotAsync();

                ConsoleWriter.WriteLine("Relevant user traits:\n" + response, LogLevel.Info, ConsoleColor.Blue);
            }
            else
            {
                ConsoleWriter.WriteLine("No user traits - skipping optimisation", LogLevel.Info, ConsoleColor.DarkGray);
            }

            return response;
        }
    }
}