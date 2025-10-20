using Hnefatafl.Console.Enums;
using Hnefatafl.Console.Tools;
using Hnefatafl.Engine.Enums;
using Hnefatafl.Engine.Models;

namespace Hnefatafl.Console
{
    internal class Program
    {
        static void Main()
        {
            do { Play(NewGame()); }
            while (Chat.GetPlayerDecision("Want to play again? [y]es/[n]o:"));
        }

        private static Game NewGame()
        {
            BoardDrawer.PrintBoard();

            var game = new Game();

            BoardDrawer.PrintFields(game.Board.Where(field => !field.IsEmpty || field.IsCorner), FieldDrawMode.Default);

            Chat.PrintCurrentPlayerPrefix();
            Chat.PrintCurrentFieldPrefix();

            return game;
        }

        private static void Play(Game game)
        {
            while (!game.IsGameOver)
            {
                Chat.PrintCurrentPlayer(game.CurrentPlayer);
                Chat.GetPlayerMove(game.Board, game.CurrentPlayer, out var pawn, out var newField);

                Field oldField = pawn.Field;
                MoveResult moveResult = game.MakeMove(pawn, newField, out _);

                if (!moveResult.HasFlag(MoveResult.PawnMoved))
                    throw new InvalidOperationException();

                BoardDrawer.PrintField(oldField, FieldDrawMode.Default);
                BoardDrawer.PrintField(newField, FieldDrawMode.Default);

                if (moveResult.HasFlag(MoveResult.OpponentPawnCaptured))
                    BoardDrawer.PrintFields(game.Board.GetAdjacentFields(newField), FieldDrawMode.Default);
            }

            Chat.PrintWinner();
        }
    }
}
