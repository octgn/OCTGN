namespace Octgn.Online.Library.Coms
{
    using System;

    using Octgn.Online.Library.Enums;

    public interface ISASToSASManagerService
    {
        void HostedGameStateChanged(Guid id, EnumHostedGameStatus status);
    }
}