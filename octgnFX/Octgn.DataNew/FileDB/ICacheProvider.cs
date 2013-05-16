namespace Octgn.DataNew.FileDB
{
    public interface ICacheProvider
    {
        bool ObjectExistsFromPath(string path);

        T GetObjectFromPath<T>(string path) where T : class;

        void AddObjectToCache(string path, object obj);

        void InvalidatePath(string path);

        void InvalidateObject(object obj);
    }
}