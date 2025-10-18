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
            public static int ColumnWidth { get; set; } = 4;
            public static int RowHeight { get; set; } = 2;
            public static ConsoleColor HeadersColor { get; set; } = ConsoleColor.White;
            public static ConsoleColor GridColor { get; set; } = ConsoleColor.DarkGray;
            public static ConsoleColor KingColor { get; set; } = ConsoleColor.Yellow;
            public static ConsoleColor DefenderColor { get; set; } = ConsoleColor.Green;
            public static ConsoleColor AttackerColor { get; set; } = ConsoleColor.Red;
            public static ConsoleColor SpecialFieldColor { get; set; } = ConsoleColor.DarkMagenta;
            public static ConsoleColor DefaultBackground { get; set; } = ConsoleColor.Black;
            public static ConsoleColor SelectionPawnBackground { get; set; } = ConsoleColor.Cyan;
            public static ConsoleColor SelectedPawnBackground { get; set; } = ConsoleColor.DarkCyan;
            public static ConsoleColor SelectionFieldBackground { get; set; } = ConsoleColor.Cyan;
        }

        private const string EMPTY_FIELD = "   ";
        private const string SEPARATOR = "---";

        private readonly static IEnumerable<char> HEADERS = Enumerable.Range(0, Board.SIZE).Select(i => ((char)('A' + i)));

        public static void PrintBoard()
        {
            int width = (Board.SIZE + 1) * Settings.ColumnWidth;
            int height = ConsoleWriter.Settings.COMMUNICATION_ROW + 4;

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
            foreach (Pawn pawn in board.GetPawns(Player.All))
                PrintField(pawn.Field);
            SetCursorPosition(0, (board.Rows + 1) * Settings.RowHeight);
        }

        public static void PrintField(Field field)
        {
            SetCursorPosition((field.Coordinates.Column.Index + 1) * 4, (field.Coordinates.Row.Index + 1) * 2);
            SetCursorColor(field.Pawn);
            Write(GetFieldText(field));
        }

        public static void PrintPawnSelection(Pawn pawn)
        {
            SetCursorPosition((pawn.Field.Coordinates.Column + 1) * 4, (pawn.Field.Coordinates.Row + 1) * 2);
            SetCursorColor(pawn);
            BackgroundColor = Settings.SelectionPawnBackground;
            Write(GetFieldText(pawn.Field));
            BackgroundColor = Settings.DefaultBackground;
        }

        public static void PrintSelectedPawn(Pawn pawn)
        {
            SetCursorPosition((pawn.Field.Coordinates.Column + 1) * 4, (pawn.Field.Coordinates.Row + 1) * 2);
            SetCursorColor(pawn);
            BackgroundColor = Settings.SelectedPawnBackground;
            Write(GetFieldText(pawn.Field));
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
