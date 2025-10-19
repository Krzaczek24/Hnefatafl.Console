using Hnefatafl.Console.Enums;
using Hnefatafl.Console.Tools;
using Hnefatafl.Engine.Enums;
using Hnefatafl.Engine.Models;
using Hnefatafl.Engine.Models.Pawns;

namespace Hnefatafl.Console
{
    internal class Program
    {
        static void Main()
        {
             BoardDrawer.PrintBoard();

            var game = new Game();
            game.Start();

            BoardDrawer.PrintFields(game.Board.Where(field => !field.IsEmpty || field.IsCorner), FieldDrawMode.Default);

            Chat.PrintCurrentPlayerPrefix();
            Chat.PrintCurrentFieldPrefix();

            while (!game.IsGameOver)
            {
                Chat.PrintCurrentPlayer(game.CurrentPlayer);
                Chat.GetPlayerMove(game.Board, out var pawn, out var newField);

                var oldField = pawn.Field;
                if (game.MakeMove(pawn, newField) is MoveResult.Success)
                {
                    BoardDrawer.PrintField(oldField, FieldDrawMode.Default);
                    BoardDrawer.PrintField(newField, FieldDrawMode.Default);
                }
            }
        }
    }
}
