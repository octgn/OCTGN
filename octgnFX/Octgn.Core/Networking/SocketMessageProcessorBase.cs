namespace Octgn.Core.Networking
{
    using System;
    using System.Collections.Generic;

    public abstract class SocketMessageProcessorBase : ISocketMessageProcessor
    {
        internal Queue<byte[]> Messages = new Queue<byte[]>();
        internal List<byte> Buffer = new List<byte>();

        public void AddData(byte[] data)
        {
            lock (this.Buffer)
            {
                this.Buffer.AddRange(data);
                var messageCount = this.ProcessBuffer(this.Buffer.ToArray());
                while (messageCount > 0)
                {
                    if (messageCount > this.Buffer.Count) throw new InvalidOperationException("Message count is greater than the available buffer size");
                    var nb = this.Buffer.GetRange(0, messageCount);
                    this.Messages.Enqueue(nb.ToArray());
                    this.Buffer.RemoveRange(0, messageCount);
                    messageCount = this.ProcessBuffer(this.Buffer.ToArray());
                }
            }
        }

        public byte[] PopMessage()
        {
            lock (this.Buffer)
            {
                return this.Messages.Count == 0 ? null : this.Messages.Dequeue();
            }
        }

        public void Clear()
        {
            lock (this.Buffer)
            {
                this.Buffer.Clear();
				this.Messages.Clear();
            }
        }

        public abstract int ProcessBuffer(byte[] data);
    }
}