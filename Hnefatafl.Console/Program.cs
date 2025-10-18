using Hnefatafl.Console.Tools;
using Hnefatafl.Engine.Models;

namespace Hnefatafl.Console
{
    internal class Program
    {
        static void Main()
        {
            BoardDrawer.PrintBoard();

            var game = new Game();
            game.Start();

            BoardDrawer.PrintPawns(game.Board);

            Chat.PrintCurrentPlayerPrefix();
            Chat.PrintCommand();

            while (!game.IsGameOver)
            {
                Chat.PrintCurrentPlayer(game.CurrentPlayer);
                var pawn = Chat.SelectPawn(game.Board);
                var field = Chat.SelectTargetField(game.Board, pawn);
            }
        }
    }
}
