namespace DiscordTest
{
    using mAIa.Bot.Chat;

    public class ConsoleWriter
    {
        public static LogLevel LogLevel = LogLevel.Debug;

        public static void WriteLine(ChatQueryResponse message, LogLevel logLevel)
        {
            if (logLevel >= LogLevel)
            {
                //WritePair("Reasoning", message.Thoughts.Reasoning);
                //WritePair("Plan", message.Thoughts.Plan);
                //WritePair("Criticism", message.Thoughts.Criticism);
                WriteLine(message.Thoughts.Speak, logLevel, ConsoleColor.Green);
            }
        }

        public static void WriteLine(string message, LogLevel logLevel, ConsoleColor color = ConsoleColor.Gray, ConsoleColor bgColor = ConsoleColor.Black)
        {
            if (logLevel >= LogLevel)
            {
                var consoleColor = Console.ForegroundColor;
                var consoleBgColor = Console.BackgroundColor;
                Console.ForegroundColor = color;
                Console.BackgroundColor = bgColor;
                Console.WriteLine(message);
                Console.ForegroundColor = consoleColor;
                Console.BackgroundColor = consoleBgColor;
            }
        }

        public static void WriteError(string text, LogLevel logLevel, ChatQueryResponse message = null)
        {
            if (logLevel >= LogLevel)
            {
                var consoleColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: " + text);

                if (message != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    WriteLine(message, logLevel);
                }

                Console.ForegroundColor = consoleColor;
            }
        }

        protected static void WritePair(string title, string text, LogLevel logLevel)
        {
            if (logLevel >= LogLevel)
            {
                var consoleColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(title + ": ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(text);
                Console.ForegroundColor = consoleColor;
            }
        }
    }

    public enum LogLevel : int
    {
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Fatal = 5
    }
}