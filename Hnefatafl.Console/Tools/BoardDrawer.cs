using Hnefatafl.Console.Enums;
using Hnefatafl.Engine.Models;
using Hnefatafl.Engine.Models.Pawns;
using KrzaqTools.Extensions;
using static System.Console;

namespace Hnefatafl.Console.Tools
{
    internal static class BoardDrawer
    {
        public static class Settings
        {
            public static int ColumnWidth { get; } = 4;
            public static int RowHeight { get; } = 2;
            public static ConsoleColor HeadersColor { get; } = ConsoleColor.White;
            public static ConsoleColor GridColor { get; } = ConsoleColor.DarkGray;
            public static ConsoleColor KingColor { get; } = ConsoleColor.Yellow;
            public static ConsoleColor DefenderColor { get; } = ConsoleColor.Green;
            public static ConsoleColor AttackerColor { get; } = ConsoleColor.Red;
            public static ConsoleColor AvailableFieldColor { get; } = ConsoleColor.DarkMagenta;
            public static ConsoleColor AvailableFieldSelectionColor { get; } = ConsoleColor.Magenta;
            public static ConsoleColor DefaultBackgroundColor { get; } = ConsoleColor.Black;
            public static ConsoleColor DefaultColor { get; } = ConsoleColor.White;
        }

        private static readonly string EMPTY_FIELD = string.Empty.PadLeft(Settings.ColumnWidth - 1, ' ');
        private static readonly string SEPARATOR = string.Empty.PadLeft(Settings.ColumnWidth - 1, '-');

        private readonly static IEnumerable<char> HEADERS = Enumerable.Range(0, Board.SIZE).Select(i => ((char)('A' + i)));

        public static void PrintBoard()
        {
            CursorVisible = false;
            BackgroundColor = Settings.DefaultBackgroundColor;

            int width = (Board.SIZE + 1) * Settings.ColumnWidth;
            int height = ConsoleWriter.Settings.CommunicationRow + 4;

            SetWindowSize(width, height);
            if (OperatingSystem.IsWindows())
                SetBufferSize(width, height);
            Clear();
            PrintGrid();
            PrintHeaders();
        }

        private static void PrintGrid()
        {
            ForegroundColor = Settings.GridColor;

            string gridCellsLine = $"{EMPTY_FIELD}|{string.Join('|', HEADERS.Select(_ => EMPTY_FIELD))}|";
            string gridSeparationLine = $"{SEPARATOR}+{string.Join('+', HEADERS.Select(_ => SEPARATOR))}+";

            for (int row = 0; row <= Board.SIZE; row++)
            {
                WriteLine(gridCellsLine);
                WriteLine(gridSeparationLine);
            }
        }

        private static void PrintHeaders()
        {
            ForegroundColor = Settings.HeadersColor;

            foreach (var (index, header) in HEADERS.Select((header, index) => (index + 1, header)))
            {
                SetCursorPosition(index * Settings.ColumnWidth + 1, 0);
                Write(header);
                SetCursorPosition(0, index * Settings.RowHeight);
                Write($"{index,2}");
            }
        }

        public static void PrintFields(IEnumerable<Field> fields, FieldDrawMode mode) => fields.ForEach(field => PrintField(field, mode));

        public static void PrintField(Field field, FieldDrawMode mode)
        {
            SetCursorPosition((field.Coordinates.Column + 1) * Settings.ColumnWidth, (field.Coordinates.Row + 1) * Settings.RowHeight);
            SetCursorColor(field.Pawn, mode);
            Write(GetFieldText(field, mode));
        }

        private static void SetCursorColor(Pawn? pawn, FieldDrawMode mode)
        {
            ForegroundColor = (pawn, mode) switch
            {
                (King, _) => Settings.KingColor,
                (Defender, _) => Settings.DefenderColor,
                (Attacker, _) => Settings.AttackerColor,
                (_, FieldDrawMode.Available) => Settings.AvailableFieldColor,
                (_, FieldDrawMode.Selection) => Settings.AvailableFieldSelectionColor,
                _ => Settings.GridColor,
            };

            if (mode is FieldDrawMode.Selection && pawn is not null)
                (ForegroundColor, BackgroundColor) = (BackgroundColor, ForegroundColor);
        }

        private static string GetFieldText(Field field, FieldDrawMode mode)
        {
            char sign = field.GetCharRepresentation();

            if ((field.IsCenter || field.IsCorner) && char.IsWhiteSpace(sign)
            || (mode is FieldDrawMode.Selection && field.IsEmpty))
                sign = 'X';

            if (mode is FieldDrawMode.Available && field.IsEmpty)
                sign = '+';

            return (mode, field.IsEmpty) switch
            {
                (FieldDrawMode.Available, false) => $"({sign})",
                (FieldDrawMode.Selected, _) => $"[{sign}]",
                _ => $" {sign} ",
            };
        }
    }
}
