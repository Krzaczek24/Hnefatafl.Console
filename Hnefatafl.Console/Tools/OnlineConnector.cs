using Hnefatafl.Console.Models;
using Krzaq.WebSocketConnector;
using Krzaq.WebSocketConnector.Interfaces;

namespace Hnefatafl.Console.Tools
{
    internal static class MoveInfoSerializerSingleton
    {
        public static MoveInfoSerializer Instance { get; } = new();
    }

    internal interface IOnlineConnector : IWebSocketConnector<MoveInfo> { }

    internal class OnlineHost() : WebSocketHost<MoveInfo>(MoveInfoSerializerSingleton.Instance), IOnlineConnector { }

    internal class OnlineClient(Uri hostAddress) : WebSocketClient<MoveInfo>(hostAddress, MoveInfoSerializerSingleton.Instance), IOnlineConnector { }
}
