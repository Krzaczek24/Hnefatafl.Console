using Hnefatafl.Console.Enums;
using Hnefatafl.Console.Tools;
using Hnefatafl.Engine.Enums;
using Hnefatafl.Engine.Models;

namespace Hnefatafl.Console
{
    internal class Program
    {
        static void Main() => Play(NewGame());

        private static Game NewGame()
        {
            BoardDrawer.PrintBoard();

            var game = new Game();
            game.Start();

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
                Chat.GetPlayerMove(game.Board, out var pawn, out var newField);

                Field oldField = pawn.Field;
                MoveResult moveResult = game.MakeMove(pawn, newField);

                if (!moveResult.HasFlag(MoveResult.PawnMoved))
                    throw new InvalidOperationException();

                BoardDrawer.PrintField(oldField, FieldDrawMode.Default);
                BoardDrawer.PrintField(newField, FieldDrawMode.Default);

                if (moveResult.HasFlag(MoveResult.OpponentPawnKilled))
                    BoardDrawer.PrintFields(game.Board.GetAdjacentFields(newField), FieldDrawMode.Default);
            }

            if (Chat.GetPlayerDecision())
                game.Start();
        }
    }
}
