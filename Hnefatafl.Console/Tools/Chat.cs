using Hnefatafl.Engine.Enums;
using Hnefatafl.Engine.Models;
using Hnefatafl.Engine.Models.Pawns;

namespace Hnefatafl.Console.Tools
{
    internal static class Chat
    {
        public static class Settings
        {
            public static ConsoleColor PrefixColor { get; set; } = ConsoleColor.DarkYellow;
            public static ConsoleColor SecondPrefixColor { get; set; } = ConsoleColor.Yellow;
        }

        const string CURRENT_PLAYER_PREFIX = "Current player: ";
        public static void PrintCurrentPlayerPrefix() => ConsoleWriter.Print(0, 0, CURRENT_PLAYER_PREFIX, Settings.PrefixColor);
        public static void PrintCurrentPlayer(Player currentPlayer)
        {
            ConsoleWriter.Print(CURRENT_PLAYER_PREFIX.Length, 0, currentPlayer, currentPlayer switch
            {
                Player.Attacker => BoardDrawer.Settings.AttackerColor,
                Player.Defender => BoardDrawer.Settings.DefenderColor,
                _ => throw new InvalidOperationException($"Unknown value of [{currentPlayer}]"),
            });
        }

        const string CHOOSE_PAWN_PREFIX = "Choose pawn to move: ";
        public static void PrintCommand() => ConsoleWriter.Print(0, 1, CHOOSE_PAWN_PREFIX, Settings.SecondPrefixColor);
        private static void ResetCursor() => System.Console.SetCursorPosition(CHOOSE_PAWN_PREFIX.Length, ConsoleWriter.Settings.COMMUNICATION_ROW + 1);

        private static string? LastErrorMessagePrinted { get; set; } = null;

        public static Pawn SelectPawn(Board board)
        {
            ResetCursor();

            Field fieldCursor = board[Board.MIDDLE_INDEX, Board.MIDDLE_INDEX];

            while (true)
            {
                ConsoleKey pressedKey = System.Console.ReadKey(true).Key;

                ClearErrorMessage();

                if (pressedKey is ConsoleKey.Enter
                               or ConsoleKey.Spacebar
                && fieldCursor.Pawn?.Player == board.Game.CurrentPlayer)
                {
                    BoardDrawer.PrintSelectedPawn(fieldCursor.Pawn);
                    ResetCursor();
                    return fieldCursor.Pawn;
                }

                if (pressedKey is ConsoleKey.LeftArrow
                               or ConsoleKey.UpArrow
                               or ConsoleKey.RightArrow
                               or ConsoleKey.DownArrow)
                {
                    BoardDrawer.PrintField(fieldCursor);
                    fieldCursor = JumpToNextPlayersPawn(board, fieldCursor, pressedKey);
                    BoardDrawer.PrintPawnSelection(fieldCursor.Pawn!);
                    ResetCursor();
                    continue;
                }

                PrintErrorMessage("Choose pawn using arrows.");
            }
        }

        public static Field SelectTargetField(Board board, Pawn pawn)
        {
            ResetCursor();

            Field fieldCursor = pawn.Field;

            while (true)
            {
                ConsoleKey pressedKey = System.Console.ReadKey(true).Key;

                ClearErrorMessage();



                PrintErrorMessage("Choose pawn using arrows.");
            }
        }

        private static Field JumpToNextPlayersPawn(Board board, Field currentField, ConsoleKey pressedKey)
        {
            var fields = board.Game.CurrentPlayerAvailablePawns.Select(x => x.Field);
            var fieldsInfo = ComputeFieldsAdditionalInfo(fields, currentField, pressedKey);
            var newSelectedField = fieldsInfo.OrderBy(x => RateField(x.Angle, x.Distance)).Select(x => x.Field).FirstOrDefault();
            return newSelectedField ?? currentField;
            static double RateField(double angle, double distance) => (angle + 1) * distance;
        }

        private static IEnumerable<(Field Field, double Angle, double Distance)> ComputeFieldsAdditionalInfo(IEnumerable<Field> fields, Field currentField, ConsoleKey pressedKey)
        {
            foreach (Field field in fields)
            {
                var (rows, columns) = field.Coordinates - currentField.Coordinates;
                switch (pressedKey)
                {
                    case ConsoleKey.LeftArrow:
                        if (columns < 0 && Math.Abs(rows) <= -columns)
                            yield return (field, Math.Atan2(Math.Abs(rows), -columns), GetDistance());
                        break;
                    case ConsoleKey.UpArrow:
                        if (rows < 0 && Math.Abs(columns) <= -rows)
                            yield return (field, Math.Atan2(Math.Abs(columns), -rows), GetDistance());
                        break;
                    case ConsoleKey.RightArrow:
                        if (columns > 0 && Math.Abs(rows) <= columns)
                            yield return (field, Math.Atan2(Math.Abs(rows), columns), GetDistance());
                        break;
                    case ConsoleKey.DownArrow:
                        if (rows > 0 && Math.Abs(columns) <= rows)
                            yield return (field, Math.Atan2(Math.Abs(columns), rows), GetDistance());
                        break;
                    default:
                        throw new InvalidOperationException($"Invalid key [{pressedKey}]");
                }

                double GetDistance() => Math.Sqrt(columns * columns + rows * rows);
            }
        }

        private static void ClearErrorMessage()
        {
            if (LastErrorMessagePrinted is not null)
            {
                ConsoleWriter.Print(0, 2, string.Empty.PadRight(LastErrorMessagePrinted.Length), ConsoleColor.DarkRed);
                ResetCursor();
                LastErrorMessagePrinted = null;
            }
        }

        private static void PrintErrorMessage(string message)
        {
            ConsoleWriter.Print(0, 2, LastErrorMessagePrinted = message, ConsoleColor.DarkRed);
            ResetCursor();
        }
    }
}
