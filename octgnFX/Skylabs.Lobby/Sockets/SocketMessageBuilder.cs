using System;
using System.Collections.Generic;

namespace Skylabs.Lobby.Sockets
{
    public class SocketMessageBuilder
    {
        private int _mbAdded;
        private byte[] _messageBuffer;
        private int _messageSize;
        private byte[] _messageSizeBuffer;
        private int _msbAdded;

        public SocketMessageBuilder()
        {
            SmQueue = new Queue<SocketMessage>();
            Restart();
        }

        public Queue<SocketMessage> SmQueue { get; set; }

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
            foreach (byte t in toadd)
            {
                if (_messageSize == -1)
                {
                    _messageSizeBuffer[_msbAdded] = t;
                    _msbAdded++;
                }
                else
                {
                    _messageBuffer[_mbAdded] = t;
                    _mbAdded++;
                }
                if (_msbAdded == 8 && _messageSize == -1)
                {
                    _messageSize = (int) BitConverter.ToInt64(_messageSizeBuffer, 0);
                    _messageBuffer = new byte[_messageSize];
                    continue;
                }
                if (_mbAdded != _messageSize) continue;
                SocketMessage sm = SocketMessage.Deserialize(_messageBuffer);
                if (sm != null)
                    SmQueue.Enqueue(sm);
                Restart();
            }
        }
    }
}