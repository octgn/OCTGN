namespace HubServer.Coms
{
    using System.Threading.Tasks;

    public interface IClientToServerComs
    {
        Task HelloBack(string returnMessage);
    }
}