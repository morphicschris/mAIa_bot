namespace DiscordTest.Commands.Handlers
{
    using Discord.WebSocket;
    using Genbox.Wikipedia;
    using Newtonsoft.Json;
    using System.Text.RegularExpressions;

    public class GoogleSearchHandler : CommandHandlerBase
    {
        public override string CommandName => "search_wikipedia";

        public override async Task<string> ExecuteAsync(Dictionary<string, object> args, SocketUserMessage message, ISocketMessageChannel imageChannel, string userMention = null)
        {
            try
            {
                var searchTerm = args["searchterm"].ToString();
                var c = new WikipediaClient();
                var articlesResponse = await c.SearchAsync(searchTerm);
                var results = new List<WikipediaSearchResult>();

                foreach (var article in articlesResponse.QueryResult.SearchResults)
                {
                    results.Add(new WikipediaSearchResult
                    {
                        Snippet = HtmlToPlainText(article.Snippet),
                        Title = article.Title,
                        Url = article.Url.ToString()
                    });
                }

                var searchResponse = new WikipediaSearchResponse
                {
                    Results = results
                };


                return JsonConvert.SerializeObject(searchResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private class WikipediaSearchResponse
        {
            public List<WikipediaSearchResult> Results { get; set; }
        }

        private class WikipediaSearchResult
        {
            public string Title { get; set; }

            public string Url { get; set; }

            public string Snippet { get; set; }
        }

        private static string HtmlToPlainText(string html)
        {
            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
            const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

            var text = html;
            //Decode html specific characters
            text = System.Net.WebUtility.HtmlDecode(text);
            //Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");
            //Replace <br /> with line breaks
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            //Strip formatting
            text = stripFormattingRegex.Replace(text, string.Empty);

            return text;
        }
    }
}