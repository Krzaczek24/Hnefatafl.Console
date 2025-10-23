using Hnefatafl.Console.Enums;
using Hnefatafl.Console.Extensions;
using Hnefatafl.Console.Models;
using Hnefatafl.Console.Tools;
using Hnefatafl.Engine.AI;
using Hnefatafl.Engine.Enums;
using Hnefatafl.Engine.Models;
using Hnefatafl.Engine.Models.Pawns;
using Hnefatafl.Engine.Online;
using Hnefatafl.Engine.Online.Interfaces;

namespace Hnefatafl.Console
{
    internal class Program
    {
        static void Main() => MainAsync().GetAwaiter().GetResult();

        static async Task MainAsync()
        {
            GameSettings? gameSettings = null;
            do
            {
                Game game = NewGame();
                if (gameSettings is null
                || !Chat.AskPlayerYesNoQuestion("With the same settings?"))
                {
                    gameSettings = Chat.GetGameSettings();
                }
                await Play(game, gameSettings);
            }
            while (Chat.AskPlayerYesNoQuestion("Want to play again?"));
        }

        private static Game NewGame()
        {
            BoardDrawer.PrintBoard();

            var game = new Game();

            BoardDrawer.PrintFields(game.Board.Where(field => !field.IsEmpty || field.IsCorner), FieldDrawMode.Default);

            Chat.PrintCurrentPlayerPrefix();
            Chat.PrintCurrentPlayer(game.CurrentSide);
            Chat.PrintCurrentFieldPrefix();

            return game;
        }

        private static async Task Play(Game game, GameSettings settings)
        {
            IGameConnector connector = settings.GameMode is GameMode.Online
                ? GetGameConnector(settings.HostAddress)
                : null!;

            while (!game.IsGameOver)
            {
                Chat.PrintCurrentPlayer(game.CurrentSide);

                Pawn movingPawn;
                Field targetField;

                if (IsLocalHumanPlayerTurn())
                {
                    Chat.GetPlayerMove(game.Board, game.CurrentSide, out movingPawn, out targetField);
                    BoardDrawer.Settings.MoveAnimation = 100;

                    if (IsOnlineGame())
                        await connector.SendMyMove(new(movingPawn.Field.Coordinates, targetField.Coordinates));
                }
                else
                {
                    Chat.PrintOpponentIsThinking();
                    Chat.ClearOpponentMove();

                    if (IsOnlineGame())
                        (await connector.WaitForOpponentMove()).Extract(game.Board, out movingPawn, out targetField);
                    else
                        AiPlayer.GetMove(game, out movingPawn, out targetField);

                    Chat.PrintOpponentMove(movingPawn.Field, targetField);
                    Chat.PrintCurrentFieldPrefix();

                    BoardDrawer.Settings.MoveAnimation = 333;
                }

                Field oldField = movingPawn.Field;
                MoveResult moveResult = game.MakeMove(movingPawn, targetField, out _);

                if (!moveResult.HasFlag(MoveResult.PawnMoved))
                    throw new InvalidOperationException();

                BoardDrawer.PrintPawnMoveAnimation(game.Board, oldField, targetField);

                if (moveResult.HasFlag(MoveResult.OpponentPawnCaptured))
                    BoardDrawer.PrintFields(game.Board.GetAdjacentFields(targetField), FieldDrawMode.Default);
            }

            Chat.PrintGameOver(game.CurrentSide, game.GameOverReason!.Value);

            bool IsLocalHumanPlayerTurn() => settings.PlayerSide.HasFlag(game.CurrentSide);
            bool IsOnlineGame() => connector is not null;
        }

        private static IGameConnector GetGameConnector(Uri? hostAddress)
        {
            return hostAddress is null ? GetGameHost() : GetGameClient(hostAddress);

            static GameHost GetGameHost()
            {
                GameHost host = new();
                host.StartAsync().GetAwaiter().GetResult();
                return host;
            }

            static GameClient GetGameClient(Uri hostAddress)
            {
                GameClient client = new(hostAddress);
                client.ConnectAsync().GetAwaiter().GetResult();
                return client;
            }
        }
    }
}
