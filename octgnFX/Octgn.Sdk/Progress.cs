using System;

namespace Octgn.Sdk
{
    public class Progress
    {
        public bool IsIndeterminate { get; set; }

        public int MaxValue { get; set; }

        public int Value { get; set; }

        public string Status { get; set; }

        public string SubStatus { get; set; }
    }
}
