﻿using System.Collections.Generic;
using Octgn.Play.Actions;
using System.Diagnostics;

namespace Octgn.Play
{
    public static class History
    {
        private static readonly List<ActionBase> history = new List<ActionBase>(128);

        public static void Record(ActionBase action)
        {
            history.Add(action);
        }

        public static void Reset()
        {
            history.Clear();
#if(DEBUG)
            Debug.WriteLine("Resetting history");
#endif
        }
    }
}