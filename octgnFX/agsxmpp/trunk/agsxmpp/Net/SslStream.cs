/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright (c) 2003-2008 by AG-Software 											 *
 * All Rights Reserved.																 *
 * Contact information for AG-Software is available at http://www.ag-software.de	 *
 *																					 *
 * Licence:																			 *
 * The agsXMPP SDK is released under a dual licence									 *
 * agsXMPP can be used under either of two licences									 *
 * 																					 *
 * A commercial licence which is probably the most appropriate for commercial 		 *
 * corporate use and closed source projects. 										 *
 *																					 *
 * The GNU Public License (GPL) is probably most appropriate for inclusion in		 *
 * other open source projects.														 *
 *																					 *
 * See README.html for details.														 *
 *																					 *
 * For general enquiries visit our website at:										 *
 * http://www.ag-software.de														 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

#if BCCRYPTO
using System;
using System.IO;

using System.Threading;

namespace agsXMPP.net
{
    /// <summary>
    /// 
    /// </summary>
    public class SslStream : Stream
    {
        Stream inStream;
        Stream outStream;

        public SslStream(Stream inputStream, Stream outputStream)
        {
            inStream    = inputStream;
            outStream   = outputStream;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            //return inStream.BeginRead(buffer, offset, count, callback, state);

            //if (!this.CanRead)
            //{
            //   // __Error.ReadNotSupported();
            //}
            SynchronousAsyncResult ar = new SynchronousAsyncResult(state, false);
            try
            {
                //int num = Read(buffer, offset, count);
                int num = inStream.Read(buffer, offset, count);
                ar.m_NumRead = num;
                ar.m_IsCompleted = true;
                //ar._waitHandle.Set();
            }
            catch (IOException exception)
            {
                //ar._exception = exception;
            }
            if (callback != null)
            {
                callback(ar);
            }
            return ar;

        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            //return inStream.EndRead(asyncResult);

            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            SynchronousAsyncResult result = asyncResult as SynchronousAsyncResult;
            if ((result == null) || result.IsWrite)
            {
                //__Error.WrongAsyncResult();
            }
            //if (result._EndXxxCalled)
            //{
            //    //__Error.EndReadCalledTwice();
            //}
            //result._EndXxxCalled = true;
            //if (result._exception != null)
            //{
            //    throw result._exception;
            //}
            return result.m_NumRead;

        }
       
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {            
            SynchronousAsyncResult ar = new SynchronousAsyncResult(state, true);
            try
            {
                //Write(buffer, offset, count);
                outStream.Write(buffer, offset, count);
                ar.m_IsCompleted = true;
                //ar._waitHandle.Set();
           
            }
            catch (IOException exception)
            {
                //ar._exception = exception;
            }
            if (callback != null)
            {
                callback(ar);
                //callback.BeginInvoke(ar, null, null);
            }
            return ar;

        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            //outStream.EndWrite(asyncResult);
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            SynchronousAsyncResult result = asyncResult as SynchronousAsyncResult;
            if ((result == null) || !result.IsWrite)
            {
                //__Error.WrongAsyncResult();
            }
            //if (result._EndXxxCalled)
            //{
            //    //__Error.EndWriteCalledTwice();
            //}
            //result._EndXxxCalled = true;
            //if (result._exception != null)
            //{
            //    throw result._exception;
            //}
        }

        public override bool CanSeek
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override bool CanRead
        {
            get { return inStream.CanRead; }
        }

        public override bool CanWrite
        {
            get { return outStream.CanWrite; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return inStream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            return inStream.ReadByte();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            outStream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            outStream.WriteByte(value);
        }

        public override long Length
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override void Flush()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override long Position
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void SetLength(long value)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
#endif