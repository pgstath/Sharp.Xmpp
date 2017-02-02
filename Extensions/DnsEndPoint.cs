using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Sharp.Xmpp.Extensions
{
    public class DnsEndPoint : Object
    {
        public DnsEndPoint(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public DnsEndPoint(string host, int port, AddressFamily addressFamily)
        {
            Host = host;
            Port = port;
            AddressFamily = addressFamily;
        }

        public AddressFamily AddressFamily { get; private set; }
        public string Host { get; private set; }
        public int Port { get; private set; }

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var p = obj as DnsEndPoint;
            if ((object)p == null)
            {
                return false;
            }

            return Host == p.Host && Port == p.Port && AddressFamily == p.AddressFamily;
        }

        public override bool Equals(DnsEndPoint p)
        {
            if ((object)p == null)
            {
                return false;
            }

            return Host == p.Host && Port == p.Port && AddressFamily == p.AddressFamily;
        }

        public override int GetHashCode()
        {
            return Host.GetHashCode() ^ Port.GetHashCode() ^ AddressFamily.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}:{1}", Host, Port);
            return sb.ToString();
        }
    }
}
