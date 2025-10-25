using Hnefatafl.Console.Enums;
using Hnefatafl.Engine.Enums;

namespace Hnefatafl.Console.Models
{
    internal static class GameSettings
    {
        public static GameMode? Mode { get; set; }
        public static Side PlayerSide { get; set; }
        public static Uri? HostAddress { get; set; }
        public static bool IsHost => HostAddress is null;
        public static void SwapSides()
        {
            if (Mode is not GameMode.Online)
                throw new InvalidOperationException("Can only swap sides in online mode.");
            PlayerSide = PlayerSide.GetOpponent();
        }
    }
}
