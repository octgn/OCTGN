namespace Shared.Coms
{
    using System.Threading.Tasks;

    public interface IServerToClientComs
    {
        Task Hello(string helloMessage);
    }
}