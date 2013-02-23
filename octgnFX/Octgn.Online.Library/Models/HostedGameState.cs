namespace Octgn.Online.Library.Models
{
    public class HostedGameState
    {
        public HostedGameSASModel GameInfo { get; set; }
        public Enums.EnumHostedGameStatus Status { get; set; }
    }
}