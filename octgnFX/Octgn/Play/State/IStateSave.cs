namespace Octgn.Play.State
{
    using System.Collections.Generic;

    public interface IStateSave
    {
        Dictionary<string, object> Values { get; }

        void SetInstance(object obj);

        object GetInstance();

        void SaveState();

        void LoadState();

        byte[] Serialize();
    }
}