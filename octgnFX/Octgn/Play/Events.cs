namespace Octgn.Play
{
    internal static class EventIds
    {
        public const int NonGame = 0;
        public const int Event = 1;
        public const int Chat = 2;
        public const int Explicit = 4;
        public const int Turn = 8;

        public const int LocalPlayer = 0;
        public const int OtherPlayer = 0x100;

        public static int PlayerFlag(Player player)
        {
            return player == Player.LocalPlayer ? LocalPlayer : OtherPlayer;
        }
    }
}