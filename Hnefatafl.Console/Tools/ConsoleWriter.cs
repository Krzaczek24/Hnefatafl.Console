using Hnefatafl.Engine.Models;
using static System.Console;

namespace Hnefatafl.Console.Tools
{
    internal static class ConsoleWriter
    {
        public class Settings
        {
            public static int CommunicationRow = (Board.SIZE + 1) * BoardDrawer.Settings.RowHeight;
            public static ConsoleColor DefaultBackgroundColor { get; } = ConsoleColor.Black;
            public static ConsoleColor DefaultForegroundColor { get; } = ConsoleColor.White;
        }

        public static void Print(int left, int top, object text, ConsoleColor foreground, bool returnCursor = false)
            => Print(left, top, text, foreground, Settings.DefaultBackgroundColor, returnCursor);
        public static void Print(int left, int top, object text, ConsoleColor foreground, ConsoleColor background, bool returnCursor = false)
        {
            SetCursorPosition(left, Settings.CommunicationRow + top);
            Print($"{text}".PadRight(BufferWidth - left), foreground, background);
            if (returnCursor)
                SetCursorPosition(left + $"{text}".Length, Settings.CommunicationRow + top);
        }

        public static void Print(object text, ConsoleColor foreground) => Print(text, foreground, Settings.DefaultBackgroundColor);
        public static void Print(object text, ConsoleColor foreground, ConsoleColor background)
        {
            SetColor(foreground, background);
            Write(text);
            SetColor(Settings.DefaultForegroundColor, Settings.DefaultBackgroundColor);
        }

        public static void SetColor(ConsoleColor foreground, ConsoleColor background)
        {
            ForegroundColor = foreground;
            BackgroundColor = background;
        }

        public static void ShowCursor() => CursorVisible = true;
        public static void HideCursor() => CursorVisible = false;
    }
}
