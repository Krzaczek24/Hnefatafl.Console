using Hnefatafl.Console.Enums;
using Hnefatafl.Console.Models;
using Hnefatafl.Engine.Enums;
using Hnefatafl.Engine.Models;
using Hnefatafl.Engine.Models.Pawns;

namespace Hnefatafl.Console.Tools
{
    internal static class Chat
    {
        public static class Settings
        {
            public static ConsoleColor DefaultColor { get; } = ConsoleColor.White;
            public static ConsoleColor FirstLineColor { get; } = ConsoleColor.DarkYellow;
            public static ConsoleColor SecondLineColor { get; } = ConsoleColor.Yellow;
            public static ConsoleColor ThirdLineColor { get; } = ConsoleColor.Cyan;
            public static ConsoleColor CoordinatesColor { get; } = ConsoleColor.Magenta;
            public static ConsoleColor ErrorColor { get; } = ConsoleColor.DarkRed;
        }

        const string CURRENT_SIDE_PREFIX = "Current side: ";
        public static void PrintCurrentPlayerPrefix() => ConsoleWriter.Print(0, 0, CURRENT_SIDE_PREFIX, Settings.FirstLineColor);
        public static void PrintCurrentPlayer(Side currentSide) => ConsoleWriter.Print(CURRENT_SIDE_PREFIX.Length, 0, currentSide, GetSideColor(currentSide));

        const string CURRENT_FIELD_PREFIX = "Current field: ";
        public static void PrintCurrentFieldPrefix() => ConsoleWriter.Print(0, 1, CURRENT_FIELD_PREFIX, Settings.SecondLineColor);
        public static void PrintCurrentField(Field? field) => ConsoleWriter.Print(CURRENT_FIELD_PREFIX.Length, 1, $"{field?.Coordinates}", Settings.CoordinatesColor);

        const string WAITING_FOR_OPPONENT_MOVE = "Waiting for opponent move ...";
        public static void PrintOpponentIsThinking() => ConsoleWriter.Print(0, 1, WAITING_FOR_OPPONENT_MOVE, Settings.SecondLineColor);

        const string OPPONENT_MOVED_PAWN = "Opponent moved pawn from {0} to {1}";
        public static void PrintOpponentMove(Field from, Field to) => ConsoleWriter.Print(0, 2, string.Format(OPPONENT_MOVED_PAWN, from.Coordinates, to.Coordinates), Settings.ThirdLineColor);
        public static void ClearOpponentMove() => ConsoleWriter.Print(0, 2, string.Empty, Settings.ThirdLineColor);

        private static string? LastErrorMessagePrinted { get; set; } = null;

        public static void GetPlayerMove(Board board, Side player, out Pawn pawn, out Field field)
        {
            pawn = null!;
            field = null!;
            while (field is null)
            {
                pawn = SelectPawn(board, player, pawn?.Field);
                field = SelectTargetField(board, pawn)!;
            }
        }

        public static Pawn SelectPawn(Board board, Side player, Field? initialField)
        {
            var availableFields = board.GetPawns(player).Where(board.CanMove).Select(pawn => pawn.Field);
            PrintCurrentField(initialField);
            Field? selectedPawnField = null;
            while (selectedPawnField is null)
                selectedPawnField = SelectField(board, availableFields, initialField, IsFromCurrentPlayerAvailablePawns, false);
            return selectedPawnField.Pawn!;
            bool IsFromCurrentPlayerAvailablePawns(Field cursor) => !cursor.IsEmpty && availableFields.Contains(cursor);
        }

        public static Field? SelectTargetField(Board board, Pawn pawn)
        {
            var availableFields = board.GetPawnAvailableFields(pawn);
            Field? selectedField = SelectField(board, availableFields, pawn.Field, fieldCursor => fieldCursor.IsEmpty, true);
            PrintCurrentField(null);
            return selectedField;
        }

        private static Field? SelectField(Board board, IEnumerable<Field> availableFields, Field? initialField, Func<Field, bool> validSelectionCondition, bool allowEscape)
        {
            foreach (Field field in availableFields)
                BoardDrawer.PrintField(field, FieldDrawMode.Available);

            if (initialField is null)
                initialField = board[Board.MIDDLE_INDEX, Board.MIDDLE_INDEX];
            else
                BoardDrawer.PrintField(initialField, FieldDrawMode.Selection);

            Field fieldCursor = initialField;
            while (true)
            {
                ConsoleKey pressedKey = System.Console.ReadKey(true).Key;

                ClearErrorMessage();

                if (pressedKey is ConsoleKey.Escape or ConsoleKey.Backspace
                && allowEscape)
                {
                    BoardDrawer.PrintFields(availableFields, FieldDrawMode.Default);
                    PrintCurrentField(null);
                    return null;
                }

                if (pressedKey is ConsoleKey.Enter or ConsoleKey.Spacebar
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

        public static ConsoleKey GetPlayerKey(string printText, params ConsoleKey[] allowedKeys)
        {
            ConsoleKey playerKey;
            do
            {
                ConsoleWriter.Print(0, 2, printText, Settings.ThirdLineColor);
                playerKey = System.Console.ReadKey(true).Key;
                ConsoleWriter.Print(0, 2, string.Empty, Settings.ThirdLineColor);
            }
            while (allowedKeys?.Length > 0 && !allowedKeys.Contains(playerKey));
            return playerKey;
        }

        public static bool AskPlayerYesNoQuestion(string question)
            => AskPlayerQuestion($"{question} [Y]es/[N]o: ", [ConsoleKey.Y, ConsoleKey.Enter], [ConsoleKey.N, ConsoleKey.Escape]);
        public static bool AskPlayerQuestion(string question, params ConsoleKey[] trueKeys) => AskPlayerQuestion(question, trueKeys, []);
        public static bool AskPlayerQuestion(string question, IEnumerable<ConsoleKey> trueKeys, IEnumerable<ConsoleKey> falseKeys)
        {
            ConsoleKey[] allowedKeys = [..trueKeys, ..falseKeys];
            if (allowedKeys.Length == 0)
                throw new ArgumentException("Provide atleast one true/false key");
            ConsoleKey playerKey = GetPlayerKey(question, allowedKeys);
            return trueKeys.Contains(playerKey);
        }

        private static Side GetPlayerSide()
        {
            bool playAsAttackers = AskPlayerQuestion("Choose your side [A]ttackers or [D]efenders: ", [ConsoleKey.A], [ConsoleKey.D]);
            return playAsAttackers ? Side.Attackers : Side.Defenders;
        }

        private static GameMode GetGameMode()
        {
            ConsoleKey key = GetPlayerKey(
                "Select mode:\n[S]ingleplayer, [H]ot-seat, [O]nline, [A]i demo",
                [ConsoleKey.S, ConsoleKey.H, ConsoleKey.O, ConsoleKey.A]);

            ConsoleWriter.Print(0, 3, string.Empty, Settings.DefaultColor);

            return key switch
            {
                ConsoleKey.S => GameMode.Singleplayer,
                ConsoleKey.H => GameMode.Hotseat,
                ConsoleKey.O => GameMode.Online,
                _ => GameMode.AiDemo,
            };
        }

        const string URI_PREFIX = "ws://";
        const string URI_SUFFIX = ":7777/";
        private static Uri GetPlayerHostUriInput()
        {
            while (true)
            {
                ConsoleWriter.Print(0, 2, "Provide host address: ", Settings.ThirdLineColor, true);

                ConsoleWriter.ShowCursor();
                string input = System.Console.ReadLine() ?? string.Empty;
#if DEBUG
                if (string.IsNullOrEmpty(input)) input = "127.0.0.1";
#endif
                ConsoleWriter.HideCursor();

                ClearErrorMessage();

                if (!input.StartsWith(URI_PREFIX)) input = $"{URI_PREFIX}{input}";
                if (!input.EndsWith(URI_SUFFIX)) input = $"{input}{URI_SUFFIX}";
                try { return new(input, UriKind.Absolute); }
                catch { PrintErrorMessage("Provide address in following format: 123.123.123.123"); }
            }
        }

        public static GameSettings GetGameSettings()
        {
            Uri? hostAddress = null;

            GameMode mode = GetGameMode();

            if (mode is GameMode.Online)
                if (!AskPlayerYesNoQuestion("Will you host game?"))
                    hostAddress = GetPlayerHostUriInput();

            Side playerSide = mode switch
            {
                GameMode.Singleplayer or GameMode.Online => GetPlayerSide(),
                GameMode.Hotseat => Side.All,
                _ => Side.None,
            };

            return new(mode, playerSide, hostAddress);
        }

        public static void PrintGameOver(Side winner, GameOverReason reason)
        {
            const string WIN_SIDE_PREFIX = "The winner side are ";
            ConsoleWriter.Print(0, 0, WIN_SIDE_PREFIX, Settings.FirstLineColor);
            ConsoleWriter.Print(WIN_SIDE_PREFIX.Length, 0, winner, GetSideColor(winner));
            ConsoleWriter.Print(0, 1, reason switch
            {
                GameOverReason.AllAttackerPawnsCaptured => "The attacking forces were defeated!",
                GameOverReason.KingCaptured => "The defenders failed to protect the king!",
                GameOverReason.KingEscaped => "The king fled from the battlefield!",
                _ => throw new InvalidOperationException()
            }, Settings.SecondLineColor);
        }

        private static void ClearErrorMessage()
        {
            if (LastErrorMessagePrinted is not null)
            {
                ConsoleWriter.Print(0, 3, string.Empty, Settings.DefaultColor);
                LastErrorMessagePrinted = null;
            }
        }

        private static void PrintErrorMessage(string message)
        {
            ClearErrorMessage();
            ConsoleWriter.Print(0, 3, LastErrorMessagePrinted = message, Settings.ErrorColor);
        }

        private static ConsoleColor GetSideColor(Side player) => player switch
        {
            Side.Attackers => BoardDrawer.Settings.AttackerColor,
            Side.Defenders => BoardDrawer.Settings.DefenderColor,
            _ => throw new InvalidOperationException($"Unknown value of [{player}]"),
        };
    }
}
