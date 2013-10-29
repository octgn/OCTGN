namespace Octgn.Play.State
{
    using System;
    using System.Collections.Generic;

	[Obsolete]
    public interface IStateSave
    {
        Dictionary<string, object> Values { get; }

        void SetInstance(object obj);

        object GetInstance();

        void SaveState();

        void LoadState();
    }
}