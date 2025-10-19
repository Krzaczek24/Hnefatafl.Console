using Hnefatafl.Console.Enums;
using Hnefatafl.Engine.Enums;
using Hnefatafl.Engine.Models;
using Hnefatafl.Engine.Models.Pawns;

namespace Hnefatafl.Console.Tools
{
    internal static class Chat
    {
        public static class Settings
        {
            public static ConsoleColor PrefixColor { get; } = ConsoleColor.DarkYellow;
            public static ConsoleColor SecondPrefixColor { get; } = ConsoleColor.Yellow;
            public static ConsoleColor CoordinatesColor { get; } = ConsoleColor.Magenta;
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

        const string CURRENT_FIELD_PREFIX = "Current field: ";
        public static void PrintCurrentFieldPrefix() => ConsoleWriter.Print(0, 1, CURRENT_FIELD_PREFIX, Settings.SecondPrefixColor);
        public static void PrintCurrentField(Field? field) => ConsoleWriter.Print(CURRENT_FIELD_PREFIX.Length, 1, $"{field?.Coordinates}".PadRight(BoardDrawer.Settings.ColumnWidth), Settings.CoordinatesColor);

        private static string? LastErrorMessagePrinted { get; set; } = null;

        public static void GetPlayerMove(Board board, out Pawn pawn, out Field field)
        {
            pawn = null!;
            field = null!;
            while (field is null)
            {
                pawn = SelectPawn(board);
                field = SelectTargetField(board, pawn)!;
            }
        }

        public static Pawn SelectPawn(Board board)
        {
            var availableFields = board.Game.CurrentPlayerAvailablePawns.Select(pawn => pawn.Field);
            Field initialField = board[Board.MIDDLE_INDEX, Board.MIDDLE_INDEX];
            Field? selectedPawnField = null;
            while (selectedPawnField is null)
                selectedPawnField = SelectField(availableFields, initialField, IsFromCurrentPlayerAvailablePawns);
            return selectedPawnField.Pawn!;
            bool IsFromCurrentPlayerAvailablePawns(Field cursor) => !cursor.IsEmpty && availableFields.Contains(cursor);
        }

        public static Field? SelectTargetField(Board board, Pawn pawn)
        {
            var availableFields = board.GetPawnAvailableFields(pawn);
            Field? selectedField = SelectField(availableFields, pawn.Field, fieldCursor => fieldCursor.IsEmpty);
            PrintCurrentField(null);
            return selectedField;
        }

        public static Field? SelectField(IEnumerable<Field> availableFields, Field initialField, Func<Field, bool> validSelectionCondition)
        {
            foreach (Field field in availableFields)
                BoardDrawer.PrintField(field, FieldDrawMode.Available);

            Field fieldCursor = initialField;
            while (true)
            {
                ConsoleKey pressedKey = System.Console.ReadKey(true).Key;

                ClearErrorMessage();

                if (pressedKey is ConsoleKey.Escape
                               or ConsoleKey.Backspace)
                {
                    return null;
                }

                if (pressedKey is ConsoleKey.Enter
                               or ConsoleKey.Spacebar
                && validSelectionCondition.Invoke(fieldCursor))
                {
                    foreach (Field field in availableFields)
                        BoardDrawer.PrintField(field, FieldDrawMode.Default);
                    BoardDrawer.PrintField(fieldCursor, FieldDrawMode.Selected);
                    return fieldCursor;
                }

                if (pressedKey is ConsoleKey.LeftArrow
                               or ConsoleKey.UpArrow
                               or ConsoleKey.RightArrow
                               or ConsoleKey.DownArrow)
                {
                    if (validSelectionCondition.Invoke(fieldCursor))
                        BoardDrawer.PrintField(fieldCursor, FieldDrawMode.Available);
                    fieldCursor = JumpToNextField(availableFields, fieldCursor, pressedKey);
                    BoardDrawer.PrintField(fieldCursor, FieldDrawMode.Selection);
                    PrintCurrentField(fieldCursor);
                    continue;
                }

                PrintErrorMessage("Use arrows or enter/space to confirm selection!");
            }
        }


        private static Field JumpToNextField(IEnumerable<Field> availableFields, Field currentField, ConsoleKey pressedKey)
        {
            var fieldsInfo = ComputeFieldsAdditionalInfo(availableFields.Except([currentField]), currentField, pressedKey);
            var newSelectedField = fieldsInfo.OrderBy(x => RateField(x.Angle, x.Distance)).Select(x => x.Field).FirstOrDefault();
            return newSelectedField ?? currentField;
            static double RateField(double angle, double distance) => Math.Pow(angle + 1, 2) * distance;
        }

        private static IEnumerable<(Field Field, double Angle, double Distance)> ComputeFieldsAdditionalInfo(IEnumerable<Field> fields, Field currentField, ConsoleKey pressedKey)
        {
            foreach (Field field in fields)
            {
                var (rows, columns) = field.Coordinates - currentField.Coordinates;
                switch (pressedKey, Math.Sign(rows), Math.Sign(columns))
                {
                    case (ConsoleKey.LeftArrow, _, -1): yield return (field, Math.Atan2(Math.Abs(rows), -columns), GetDistance()); break;
                    case (ConsoleKey.UpArrow, -1, _): yield return (field, Math.Atan2(Math.Abs(columns), -rows), GetDistance()); break;
                    case (ConsoleKey.RightArrow, _, 1): yield return (field, Math.Atan2(Math.Abs(rows), columns), GetDistance()); break;
                    case (ConsoleKey.DownArrow, 1, _): yield return (field, Math.Atan2(Math.Abs(columns), rows), GetDistance()); break;
                }
                double GetDistance() => Math.Sqrt(rows * rows + columns * columns);
            }
        }

        private static void ClearErrorMessage()
        {
            if (LastErrorMessagePrinted is not null)
            {
                ConsoleWriter.Print(0, 2, string.Empty.PadRight(LastErrorMessagePrinted.Length), ConsoleColor.DarkRed);
                LastErrorMessagePrinted = null;
            }
        }

        private static void PrintErrorMessage(string message)
        {
            ConsoleWriter.Print(0, 2, LastErrorMessagePrinted = message, ConsoleColor.DarkRed);
        }
    }
}
