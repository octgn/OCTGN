namespace Octgn.Play
{
    using System;
    using System.Threading;

    public class PlayDispatcherAction
    {
        private readonly Delegate _delegate;
        private readonly object[] args;
        private bool finished;
        public bool HasReturnValue { get; private set; }
        public object ReturnValue { get; private set; }

        public PlayDispatcherAction(Delegate d, params object[] args)
        {
            this.HasReturnValue = d.Method.ReturnType != typeof(void);
            this._delegate = d;
            this.args = args;
        }

        public void Invoke()
        {
            this.ReturnValue = this._delegate.DynamicInvoke(this.args);
            this.finished = true;
        }

        public object Wait()
        {
            while (!this.finished)
            {
                Thread.Sleep(1);
            }

            return ReturnValue;
        }
    }
}