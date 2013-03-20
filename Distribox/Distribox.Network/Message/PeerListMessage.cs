﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distribox.Network
{
    class PeerListMessage : ProtocolMessage
    {
        public PeerList List;

        public PeerListMessage(PeerList list, int port) : base(port)
        {
            List = list;
            _type = MessageType.PeerListMessage;
        }
    }
}