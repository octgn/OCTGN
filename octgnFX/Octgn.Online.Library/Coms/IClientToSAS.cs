namespace Octgn.Online.Library.Coms
{
    using System;
    using System.Threading.Tasks;

    public interface IClientToSAS
    {
        Task Hello(string nick, Guid key, Version octgnOnlineLibraryVersion, Version gameVersion, string password);

        //Task Pick
    }
}