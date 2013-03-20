﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Distribox.Network
{
    class Peer
    {
        private IPAddress _address;
        private int _port;

        public Peer(IPAddress _address, int _port)
        {
            this._address = _address;
            this._port = _port;
        }

        public String IP
        {
            get
            {
                return _address.ToString();
            }

            set
            {
                _address = IPAddress.Parse(value);
            }
        }

        public int Port
        {
            get
            {
                return _port;
            }

            set
            {
                _port = value;
            }
        }

        public override int GetHashCode()
        {
            return String.Format("{0}:{1}", IP, Port).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var peer = (Peer)obj;
            return this.IP == peer.IP && this.Port == peer.Port;
        }
    }

    class PeerList
    {
        // FIXME: I set all of these public for JSON serialize
        public HashSet<Peer> _peers;
        private Random _random = new Random();

        public string _peerFileName;

        private void FlushToDisk()
        {
            CommonLib.CommonHelper.WriteObject(_peerFileName, this);
        }

        public PeerList(string peerFileName)
        {
            _peerFileName = peerFileName;
            _peers = new HashSet<Peer>();
        }

        static public PeerList GetPeerList(string peerFileName)
        {
            if (File.Exists(peerFileName))
                return CommonLib.CommonHelper.ReadObject<PeerList>(peerFileName);
            else
                return new PeerList(peerFileName);
        }

        public void AddPeerAndFlush(Peer peer)
        {
            AddPeer(peer);

            FlushToDisk();
        }

        private void AddPeer(Peer peer)
        {
            if (!(_peers.Contains(peer)))
            {
                _peers.Add(peer);
                //_peers_ram.Add(peer);

                System.Console.WriteLine("New peer: {0}!", peer.IP);
            }
        }

        public Peer SelectRandomPeer()
        {
            ArrayList peers_ram = new ArrayList();
            foreach (Peer peer in _peers)
                peers_ram.Add(peer);

            if (peers_ram.Count == 0)
                return null;
            return (Peer)peers_ram[_random.Next(peers_ram.Count)];
        }

        public void MergeWith(PeerList list)
        {
            foreach (Peer peer in list._peers)
                if (!_peers.Contains(peer))
                    AddPeer(peer);

            FlushToDisk();
        }

        public string ToJSON()
        {
            return CommonLib.CommonHelper.Show(this);
        }

        static public PeerList ParseJSON(string json)
        {
            return (PeerList)CommonLib.CommonHelper.Read<PeerList>(json);
        }
    }
}