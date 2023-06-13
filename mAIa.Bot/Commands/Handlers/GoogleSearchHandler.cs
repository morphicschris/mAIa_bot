//using Discord.WebSocket;
//using Google.Apis.Customsearch.v1;
//using Google.Apis.Services;
//using Newtonsoft.Json;

//namespace DiscordTest.Commands.Handlers
//{
//    public class GoogleSearchHandler : CommandHandlerBase
//    {
//        public override string CommandName => "google";

//        public override async Task<string> ExecuteAsync(Dictionary<string, object> args, SocketUserMessage message, string userMention = null)
//        {
//            try
//            {
//                var apiKey = Settings.GOOGLE_APIKEY;
//                var searchEngineId = "825999d92c826465e";

//                var customSearchService = new CustomsearchService(new BaseClientService.Initializer
//                {
//                    ApplicationName = "mAIa",
//                    ApiKey = apiKey
//                });

//                CseResource.ListRequest searchRequest = customSearchService.Cse.List();
//                searchRequest.Cx = searchEngineId;
//                searchRequest.Q = args["input"].ToString();
//                searchRequest.Gl = "GB";
//                searchRequest.Num = 5;

//                var bgColor = Console.ForegroundColor;
//                Console.ForegroundColor = ConsoleColor.Green;
//                Console.WriteLine($"Running Google search for \"{args["input"].ToString()}\"");
//                Console.ForegroundColor = bgColor;

//                var searchResults = await searchRequest.ExecuteAsync();

//                var response = new GoogleSearchResponse
//                {
//                    Results = new List<GoogleSearchResult>()
//                };

//                if (searchResults.Items != null && searchResults.Items.Any())
//                {
//                    foreach (var item in searchResults.Items)
//                    {
//                        response.Results.Add(new GoogleSearchResult
//                        {
//                            Title = item.Title,
//                            Url = item.Link,
//                            //Summary = item.Snippet
//                        });
//                    }

//                    Console.WriteLine("Results:\n\n" + string.Join("\n", searchResults.Items.Select(e => e.Title)));

//                    return JsonConvert.SerializeObject(response);
//                }
//                else
//                {
//                    Console.WriteLine("No results found");

//                    return "No results found";
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//                throw;
//            }
//        }

//        private class GoogleSearchResponse
//        {
//            public List<GoogleSearchResult> Results { get; set; }
//        }

//        private class GoogleSearchResult
//        {
//            public string Title { get; set; }
//            public string Url { get; set; }
//            //public string Summary { get; set; }
//        }
//    }
//}