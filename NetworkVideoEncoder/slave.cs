using MD5_V4._0_C;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkVideoEncoder
{
    class slave
    {
        private int port;
        private tcpSlave s;

        public slave(int port)
        {
            this.port = port;
            listenToMaster()
        }

        private void listenToMaster()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            TcpClient client = listener.AcceptTcpClient();
            NetworkStream NStream = client.GetStream();
            listener = null;

            s = new tcpSlave(client, NStream);

            while (true)
            {
                recieveJob();
            }
        }

        private void recieveJob()
        {
            long size;
            string extension;
            string fileInfo = ":FileLenght=" + size + "::Extension=" + extension + ":";

            s.Recieve(out size , out extension);
        }
    }
}
