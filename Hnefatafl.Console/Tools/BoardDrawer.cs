using Hnefatafl.Engine.Enums;
using Hnefatafl.Engine.Models;
using Hnefatafl.Engine.Models.Pawns;
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
            public static ConsoleColor SpecialFieldColor { get; } = ConsoleColor.DarkMagenta;
            public static ConsoleColor DefaultBackground { get; } = ConsoleColor.Black;
            public static ConsoleColor AvailablePawnBackground { get; } = ConsoleColor.DarkCyan;
            public static ConsoleColor SelectionPawnBackground { get; } = ConsoleColor.Cyan;
            public static ConsoleColor SelectedPawnBackground { get; } = ConsoleColor.DarkCyan;
            public static ConsoleColor AvailableFieldBackground { get; } = ConsoleColor.DarkCyan;
            public static ConsoleColor SelectionFieldBackground { get; } = ConsoleColor.Cyan;
        }

        private static readonly string EMPTY_FIELD = string.Empty.PadLeft(Settings.ColumnWidth - 1, ' ');
        private static readonly string SEPARATOR = string.Empty.PadLeft(Settings.ColumnWidth - 1, '-');

        private readonly static IEnumerable<char> HEADERS = Enumerable.Range(0, Board.SIZE).Select(i => ((char)('A' + i)));

        public static void PrintBoard()
        {
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

        public static void PrintPawns(Board board)
        {
            foreach (Pawn pawn in board.GetPawns(Player.All, false))
                PrintField(pawn.Field, Settings.DefaultBackground);
        }

        public static void PrintPawnAvailability(Pawn pawn) => PrintField(pawn.Field, Settings.AvailablePawnBackground);
        public static void PrintPawnSelection(Pawn pawn) => PrintField(pawn.Field, Settings.SelectionPawnBackground);
        public static void PrintSelectedPawn(Pawn pawn) => PrintField(pawn.Field, Settings.SelectedPawnBackground);

        public static void PrintFieldAvailability(Field field) => PrintField(field, Settings.AvailableFieldBackground);
        public static void PrintFieldSelection(Field field) => PrintField(field, Settings.SelectionFieldBackground);
        public static void PrintField(Field field, ConsoleColor background)
        {
            SetCursorPosition((field.Coordinates.Column + 1) * Settings.ColumnWidth, (field.Coordinates.Row + 1) * Settings.RowHeight);
            SetCursorColor(field.Pawn);
            BackgroundColor = background;
            Write(GetFieldText(field));
            BackgroundColor = Settings.DefaultBackground;
        }

        private static void SetCursorColor(Pawn? pawn)
        {
            ForegroundColor = pawn switch
            {
                King => Settings.KingColor,
                Defender => Settings.DefenderColor,
                Attacker => Settings.AttackerColor,
                _ => Settings.SpecialFieldColor,
            };
        }

        private static string GetFieldText(Field field)
        {
            char sign = field.GetCharRepresentation();
            if (char.IsWhiteSpace(sign)
            && (field.IsCenter || field.IsCorner))
                sign = 'X';
            return $" {sign} ";
        }
    }
}
