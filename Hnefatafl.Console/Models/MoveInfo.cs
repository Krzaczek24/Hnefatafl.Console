using Hnefatafl.Engine.Models;
using Hnefatafl.Engine.Models.Pawns;
using Krzaq.WebSocketConnector.Interfaces;

namespace Hnefatafl.Console.Models
{
    public record MoveInfo(Coordinates From, Coordinates To);

    internal class MoveInfoSerializer : ISerializer<MoveInfo>
    {
        private const char SEPRARATOR = '|';

        public string Serialize(MoveInfo moveInfo) => $"{moveInfo.From}{SEPRARATOR}{moveInfo.To}";

        public MoveInfo Deserialize(string text)
        {
            string[] labels = text.Split(SEPRARATOR);
            return new(new(labels[0]), new(labels[1]));
        }
    }

    internal static class MoveInfoExtensions
    {
        public static void Extract(this MoveInfo moveInfo, Board board, out Pawn movedPawn, out Field targetField)
        {
            movedPawn = board[moveInfo.From].Pawn!;
            targetField = board[moveInfo.To];
        }
    }
}
