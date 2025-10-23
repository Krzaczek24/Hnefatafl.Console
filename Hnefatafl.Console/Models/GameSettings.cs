using Hnefatafl.Console.Enums;
using Hnefatafl.Engine.Enums;
using System.Net;

namespace Hnefatafl.Console.Models
{
    internal record GameSettings(GameMode GameMode, Side PlayerSide, Uri? HostAddress);
}
