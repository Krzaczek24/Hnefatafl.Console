using Hnefatafl.Console.Enums;
using Hnefatafl.Console.Models;
using Hnefatafl.Console.Tools;
using Hnefatafl.Engine.AI;
using Hnefatafl.Engine.Enums;
using Hnefatafl.Engine.Models;
using Hnefatafl.Engine.Models.Pawns;

namespace Hnefatafl.Console
{
    internal class Program
    {
        static void Main() => MainAsync().GetAwaiter().GetResult();

        static async Task MainAsync()
        {
            do
            {
                Game game = NewGame();

                if (GameSettings.Mode is null)
                    Chat.GetGameSettings();

                else if (GameSettings.Mode is GameMode.Online && GameSettings.IsHost && Chat.AskPlayerYesNoQuestion("Want to swap sides?"))
                    GameSettings.SwapSides();

                else if (Chat.AskPlayerYesNoQuestion("Want to change game settings?"))
                    Chat.GetGameSettings();

                await Play(game);
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

        private static async Task Play(Game game)
        {
            var connector = GameSettings.Mode is GameMode.Online
                ? await InitializeOnlineConnection()
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

            bool IsLocalHumanPlayerTurn() => GameSettings.PlayerSide.HasFlag(game.CurrentSide);
            bool IsOnlineGame() => connector is not null;
        }

        private static Task<IOnlineConnector> InitializeOnlineConnection()
        {
            MoveInfo hostIsAttackerSignal = new(new(2,1), new(3, 7));
            MoveInfo hostIsDefenderSignal = new(new(1,9), new(9, 4));

            return GameSettings.IsHost ? InitializeGameHost() : InitializeGameClient();

            async Task<IOnlineConnector> InitializeGameHost()
            {
                var host = new OnlineHost();
                Chat.PrintWaitingForClientToConnect();
                await host.StartAsync();
                Chat.PrintClientConnected();

                MoveInfo hostSignal = GameSettings.PlayerSide is Side.Attackers ? hostIsAttackerSignal : hostIsDefenderSignal;
                await host.Send(hostSignal);

                return host;
            }

            async Task<IOnlineConnector> InitializeGameClient()
            {
                var client = new OnlineClient(GameSettings.HostAddress!);
                Chat.PrintConnectingToHost();
                await client.ConnectAsync();
                Chat.PrintConnectedToHost();

                MoveInfo hostSignal = await client.WaitForResponse();
                GameSettings.PlayerSide = hostSignal == hostIsAttackerSignal ? Side.Defenders : Side.Attackers;

                return client;
            }
        }
    }
}
