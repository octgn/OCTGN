using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skylabs.Net;

namespace Skylabs.Lobby.Sockets
{
    public class SocketMessageBuilder
    {
        public Queue<SocketMessage> SMQueue { get; set; }
        private byte[] _messageSizeBuffer;
        private byte[] _messageBuffer;
        private int _messageSize;
        private int _msbAdded;
        private int _mbAdded;
        public SocketMessageBuilder()
        {
            SMQueue = new Queue<SocketMessage>();
            Restart();
        }
        private void Restart()
        {
            _messageSizeBuffer = new byte[8];
            _messageBuffer = null;
            _messageSize = -1;
            _msbAdded = 0;
            _mbAdded = 0;
        }
        public void AddBytes(byte[] toadd)
        {
            for (int i = 0; i < toadd.Length; i++)
            {
                if (_messageSize == -1)
                {
                    _messageSizeBuffer[_msbAdded] = toadd[i];
                    _msbAdded++;
                }
                else
                {
                    _messageBuffer[_mbAdded] = toadd[i];
                    _mbAdded++;
                }
                if (_msbAdded == 8 && _messageSize == -1)
                {
                    _messageSize = (int)BitConverter.ToInt64(_messageSizeBuffer, 0);
                    _messageBuffer = new byte[_messageSize];
                    continue;
                }
                if (_mbAdded == _messageSize)
                {
                    SocketMessage sm = SocketMessage.Deserialize(_messageBuffer);
                    if (sm != null)
                        SMQueue.Enqueue(sm);
                    Restart();
                }
            }
        }
    }
}
