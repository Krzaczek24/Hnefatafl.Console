using Hnefatafl.Engine.Models;
using Hnefatafl.Engine.Models.Pawns;
using Hnefatafl.Engine.OnlineConnector.Models;

namespace Hnefatafl.Console.Extensions
{
    internal static class MoveInfoExtensions
    {
        public static void Extract(this MoveInfo moveInfo, Board board, out Pawn movedPawn, out Field targetField)
        {
            movedPawn = board[moveInfo.From].Pawn!;
            targetField = board[moveInfo.To];
        }
    }
}
