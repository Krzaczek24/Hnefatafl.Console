using Hnefatafl.Engine.Enums;
using Hnefatafl.Engine.Models;
using Hnefatafl.Engine.Models.Pawns;
using KrzaqTools.Extensions;

namespace Hnefatafl.Engine.AI.Tests
{
    public class AiTests
    {
        [Test]
        public void GetMove_ReturnsValidMove_OnNewGame()
        {
            // --- Arrange ---
            Game game = new();

            // --- Act ---
            AiPlayer.GetMove(game, out Pawn pawn, out Field field);

            // --- Assert ---
            Assert.Multiple(() =>
            {
                Assert.That(pawn, Is.Not.Null);
                Assert.That(field, Is.Not.Null);
                Assert.That(game.CurrentPlayerAvailablePawns, Contains.Item(pawn));
                Assert.That(game.CanMakeMove(pawn, field), Is.EqualTo(MoveValidationResult.Success));
            });
        }

        [Test]
        public void GetMove_ReturnsValidMove_ToFirstPawnCapture()
        {
            // --- Arrange ---
            Game game = new();
            MoveResult result = default;

            // --- Act ---
            while (!game.IsGameOver && !result.HasFlag(MoveResult.OpponentPawnCaptured))
            {
                AiPlayer.GetMove(game, out Pawn pawn, out Field field);
                result = game.MakeMove(pawn, field, out MoveValidationResult validation);
                Assert.That(validation, Is.EqualTo(MoveValidationResult.Success));
            }

            // --- Assert ---
            Assert.Pass("Expected at least one capture or the game to finish within the move limit.");
        }

        [Test]
        public void GetMove_ThrowsInvalidOperation_WhenGameIsOver()
        {
            // --- Arrange ---
            Game game = new();
            MoveResult lastMove = default;

            // Play a short sequence of moves that ends the game (king captured)
            // Sequence adapted from existing tests
            var moves = new (string From, string To)[]
            {
                ("e1", "e4"), ("f4", "j4"),
                ("g1", "g3"), ("f5", "f4"),
                ("d1", "d3"), ("f4", "i4"),
                ("d3", "e3"), ("f6", "f3"),
                ("e4", "f4")
            };

            // --- Act ---
            moves.ForEach(move => lastMove = game.MakeMove(move.From, move.To));

            // --- Act & Assert ---
            Assert.Multiple(() =>
            {
                Assert.That(game.IsGameOver, Is.True, "Expected game to be over after the sequence of moves.");
                Assert.That(lastMove, Is.EqualTo(MoveResult.KingCaptured));
                Assert.Throws<InvalidOperationException>(() => AiPlayer.GetMove(game, out _, out _));
            });
        }
    }

    public static class TestGameExtensions
    {
        public static MoveResult MakeMove(this Game game, string from, string to)
        {
            Pawn movingPawn = game.Board[new(from)].Pawn!;
            Field targetField = game.Board[new(to)];
            return game.MakeMove(movingPawn, targetField, out _);
        }
    }
}
