using Hnefatafl.Engine.Models;
using static System.Console;

namespace Hnefatafl.Console.Tools
{
    internal static class ConsoleWriter
    {
        public class Settings
        {
            public const int COMMUNICATION_ROW = (Board.SIZE + 1) * 2;
            public static ConsoleColor DefaultColor { get; set; } = ConsoleColor.White;
        }

        public static void Print(int left, int top, object text, ConsoleColor color, ConsoleColor background = ConsoleColor.Black)
        {
            SetCursorPosition(left, Settings.COMMUNICATION_ROW + top);
            Print(text, color, background);
        }

        public static void Print(object text, ConsoleColor color, ConsoleColor background = ConsoleColor.Black)
        {
            ForegroundColor = color;
            BackgroundColor = background;
            Write(text);
            ForegroundColor = Settings.DefaultColor;
            BackgroundColor = ConsoleColor.Black;
        }
    }
}
