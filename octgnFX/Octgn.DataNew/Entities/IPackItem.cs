namespace Octgn.DataNew.Entities
{
    public interface IPackItem
    {
        PackContent GetCards(Pack pack, Set set);
    }
}