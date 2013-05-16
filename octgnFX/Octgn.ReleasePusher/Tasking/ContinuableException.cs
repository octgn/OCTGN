namespace Octgn.ReleasePusher.Tasking
{
    using System;

    /// <summary>
    /// Wrap an <see cref="System.Exception"/> here to tell the <see cref="TaskManager"/> to continue, but mark the task as failed.
    /// </summary>
    public class ContinuableException : Exception
    {
    }
}
