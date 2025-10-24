using Hnefatafl.Console.Enums;
using Hnefatafl.Console.Models;
using Hnefatafl.Console.Tools;
using Hnefatafl.Engine.AI;
using Hnefatafl.Engine.Enums;
using Hnefatafl.Engine.Models;
using Hnefatafl.Engine.Models.Pawns;
using Krzaq.WebSocketConnector.Interfaces;

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
            IWebSocketConnector<MoveInfo> connector = settings.GameMode is GameMode.Online
                ? await GetGameConnector(settings.HostAddress)
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
                        await connector.Send(new(movingPawn.Field.Coordinates, targetField.Coordinates));
                }
                else
                {
                    Chat.PrintOpponentIsThinking();
                    Chat.ClearOpponentMove();

                    if (IsOnlineGame())
                    {
                        MoveInfo moveInfo = await connector.WaitForResponse();
                        moveInfo.Extract(game.Board, out movingPawn, out targetField);
                    }
                    else
                    {
                        AiPlayer.GetMove(game, out movingPawn, out targetField);
                    }

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

        private static Task<IOnlineConnector> GetGameConnector(Uri? hostAddress)
        {
            return hostAddress is null ? GetGameHost() : GetGameClient(hostAddress);

            async Task<IOnlineConnector> GetGameHost()
            {
                var host = new OnlineHost();
                await host.StartAsync();
                return host;
            }

            async Task<IOnlineConnector> GetGameClient(Uri hostAddress)
            {
                var client = new OnlineClient(hostAddress);
                await client.ConnectAsync();
                return client;
            }
        }
    }
}
