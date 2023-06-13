namespace DiscordTest.Commands.Handlers
{
    using Discord.WebSocket;
    using HtmlAgilityPack;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class BrowseWebsiteHandler : CommandHandlerBase
    {
        public override string CommandName => "browse_website";

        public override async Task<string> ExecuteAsync(Dictionary<string, object> args, SocketUserMessage message, ISocketMessageChannel imageChannel, string userMention = null)
        {
            if (!args.TryGetValue("url", out var urlObj) || !(urlObj is string url))
            {
                return "Invalid arguments: URL not found or is not a string";
            }

            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return $"Error: {response.StatusCode}";
                }

                var content = await response.Content.ReadAsStringAsync();
                var htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(content);

                var textContent = ExtractTextFromHtml(htmlDocument.DocumentNode);
                return textContent;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private string ExtractTextFromHtml(HtmlNode node)
        {
            if (node == null)
            {
                return "";
            }

            if (node.NodeType == HtmlNodeType.Text)
            {
                return node.InnerText;
            }

            string result = "";

            var childNodes = node.Descendants()
                .Where(n =>
                    n.NodeType == HtmlNodeType.Text &&
                    n.ParentNode.Name != "script" &&
                    n.ParentNode.Name != "style"
                ).Select(n => n.InnerText.ToLower());

            foreach (var childNode in childNodes)
            {
                result += childNode + " ";
            }

            return result;
        }
    }

}