using Hnefatafl.Console.Enums;
using Hnefatafl.Console.Tools;
using Hnefatafl.Engine.AI;
using Hnefatafl.Engine.Enums;
using Hnefatafl.Engine.Models;
using Hnefatafl.Engine.Models.Pawns;

namespace Hnefatafl.Console
{
    internal class Program
    {
        static void Main()
        {
            do { Play(NewGame(out AiPlayer ai), ai); }
            while (Chat.AskPlayerQuestion("Want to play again? [y]es/[n]o:", [ConsoleKey.Y, ConsoleKey.Enter]));
        }

        private static Game NewGame(out AiPlayer ai)
        {
            BoardDrawer.PrintBoard();

            var game = new Game();

            BoardDrawer.PrintFields(game.Board.Where(field => !field.IsEmpty || field.IsCorner), FieldDrawMode.Default);

            Chat.PrintCurrentPlayerPrefix();
            Chat.PrintCurrentPlayer(game.CurrentSide);
            Chat.PrintCurrentFieldPrefix();

            Side playerSide = Side.All;

            AiLevel? aiLevel = Chat.GetAiLevel();
            if (aiLevel is not null)
                playerSide = Chat.GetPlayerSide();

            ai = new(aiLevel ?? default, playerSide ^ Side.All);

            return game;
        }

        private static void Play(Game game, AiPlayer ai)
        {
            Side playerSide = ai.Side ^ Side.All;

            while (!game.IsGameOver)
            {
                bool isHumanPlayer = playerSide.HasFlag(game.CurrentSide);
                Chat.PrintCurrentPlayer(game.CurrentSide);

                Pawn pawn;
                Field newField;

                if (isHumanPlayer)
                    Chat.GetPlayerMove(game.Board, game.CurrentSide, out pawn, out newField);
                else
                {
                    Chat.PrintAiIsThinking();
                    Chat.ClearAiMove();
                    ai.GetMove(game, out pawn, out newField);
                    Chat.PrintAiMove(pawn.Field, newField);
                    Chat.PrintCurrentFieldPrefix();
                }

                Field oldField = pawn.Field;
                MoveResult moveResult = game.MakeMove(pawn, newField, out _);

                if (!moveResult.HasFlag(MoveResult.PawnMoved))
                    throw new InvalidOperationException();

                BoardDrawer.PrintPawnMoveAnimation(game.Board, oldField, newField, isHumanPlayer ? 100 : 333);

                if (moveResult.HasFlag(MoveResult.OpponentPawnCaptured))
                    BoardDrawer.PrintFields(game.Board.GetAdjacentFields(newField), FieldDrawMode.Default);
            }

            Chat.PrintGameOver(game.CurrentSide, game.GameOverReason!.Value);
        }
    }
}
