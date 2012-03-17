#region

using System;

#endregion

namespace CassiniDev.ServerLog
{
    /// <summary>
    /// </summary>
    [Serializable]
    public class LogInfo : ICloneable
    {
        public byte[] Body { get; set; }

        public Guid ConversationId { get; set; }

        public DateTime Created { get; set; }

        public string Exception { get; set; }

        public string Headers { get; set; }

        public string Identity { get; set; }

        public string PathTranslated { get; set; }

        public string PhysicalPath { get; set; }

        public long RowId { get; set; }

        public long RowType { get; set; }

        public long? StatusCode { get; set; }

        public string Url { get; set; }

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        #endregion

        public LogInfo Clone()
        {
            var result = (LogInfo) ((ICloneable) this).Clone();
            if (Body != null)
            {
                result.Body = new byte[Body.Length];
                Body.CopyTo(result.Body, 0);
            }

            return result;
        }
    }
}