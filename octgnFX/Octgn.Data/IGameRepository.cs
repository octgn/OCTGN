namespace Octgn.Data
{
    using System.Linq;

    /// <summary>
    /// Interface for creating a game repository
    /// </summary>
    public interface IGameRepository
    {
        /// <summary>
        /// List of installed games.
        /// </summary>
        IQueryable<Database.Entities.Game> Games { get; }
    }
}
